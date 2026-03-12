# AIRelief — AWS Deployment Guide (ECS Fargate + ALB + RDS)

This guide walks through deploying the AIRelief application to AWS using **ECS Fargate** (containerised compute), **ALB** (Application Load Balancer), and **RDS PostgreSQL** (managed database).

---

## Architecture Overview

```
Internet
   ?
   ?
Route 53 (DNS)  ???  ACM Certificate (HTTPS)
   ?
   ?
Application Load Balancer (ALB)
   ?  ???? Sticky Sessions (cookie-based) ????
   ?                                          ?
ECS Fargate Task 1                    ECS Fargate Task 2  ...
   ?        ?                           ?        ?
   ?        ??? EFS /app/keys ???????????        ?
   ?           (shared Data Protection keys)     ?
   ?                                             ?
               RDS PostgreSQL (shared)
```

**Key design decisions:**
- **No Redis** — removed from codebase
- **ALB sticky sessions** ensure a user's requests go to the same container (protects in-memory session for anonymous Trial flow)
- **EFS volume** shares Data Protection keys across containers so auth cookies work everywhere
- **RDS PostgreSQL** is the single database (same as local dev, just managed)
- **Authenticated users' lesson state** is already in the database (`ActiveLessons` table), so scaling containers does NOT disrupt logged-in users

---

## Multi-Container Scaling Analysis

| Feature | State Storage | Scaling Impact | Mitigation |
|---|---|---|---|
| **Login / Auth cookies** | Data Protection keys | Keys must be shared across containers | ? EFS-mounted `/app/keys` directory |
| **Trial quiz (anonymous)** | In-memory session | Lost if request goes to different container | ? ALB sticky sessions (cookie affinity) |
| **Lesson progress (logged-in)** | Database (`ActiveLessons` table) | No impact — fully database-backed | ? Already safe |
| **User statistics** | Database (`UserStatistics` table) | No impact | ? Already safe |
| **TempData** | Cookie-based (default) | No impact — travels with the request | ? Already safe |

**Bottom line:** With ALB sticky sessions + EFS for keys, scaling to multiple containers is safe for both anonymous and authenticated users.

---

## Prerequisites

- AWS account with admin access
- AWS CLI v2 installed and configured (`aws configure`)
- Docker installed locally
- A domain name (optional but recommended for HTTPS)

---

## Step-by-Step Deployment

### STEP 1 — Create a VPC (if you don't have one) `[MANUAL — AWS Console]`

1. Go to **VPC Console** ? **Create VPC**
2. Choose **VPC and more** (creates subnets, route tables, internet gateway automatically)
3. Settings:
   - Name: `airelief-vpc`
   - IPv4 CIDR: `10.0.0.0/16`
   - Number of AZs: **2** (minimum for ALB)
   - Public subnets: **2**
   - Private subnets: **2**
   - NAT Gateways: **1 per AZ** (or "In 1 AZ" to save cost)
4. Click **Create VPC**

> ?? Note the VPC ID, public subnet IDs, and private subnet IDs — you'll need them throughout.

---

### STEP 2 — Create Security Groups `[MANUAL — AWS Console]`

Create **3 security groups** in your VPC:

#### 2a. ALB Security Group (`airelief-alb-sg`)
| Type | Port | Source |
|------|------|--------|
| Inbound HTTP | 80 | 0.0.0.0/0 |
| Inbound HTTPS | 443 | 0.0.0.0/0 |
| Outbound All | All | 0.0.0.0/0 |

#### 2b. ECS Tasks Security Group (`airelief-ecs-sg`)
| Type | Port | Source |
|------|------|--------|
| Inbound Custom TCP | 8080 | `airelief-alb-sg` (the ALB SG) |
| Outbound All | All | 0.0.0.0/0 |

#### 2c. RDS Security Group (`airelief-rds-sg`)
| Type | Port | Source |
|------|------|--------|
| Inbound PostgreSQL | 5432 | `airelief-ecs-sg` (the ECS SG) |
| Outbound All | All | 0.0.0.0/0 |

---

### STEP 3 — Create RDS PostgreSQL Database `[MANUAL — AWS Console]`

1. Go to **RDS Console** ? **Create database**
2. Settings:
   - Engine: **PostgreSQL** (version 16.x recommended)
   - Template: **Free tier** (for testing) or **Production**
   - DB instance identifier: `airelief-db`
   - Master username: `airelief`
   - Master password: *(choose a strong password — save it)*
   - DB instance class: `db.t3.micro` (free tier) or `db.t3.small`+
   - Storage: 20 GB gp3
   - VPC: `airelief-vpc`
   - Subnet group: Create new or use existing private subnets
   - Public access: **No**
   - Security group: `airelief-rds-sg`
   - Initial database name: `aireliefdb`
3. Click **Create database**
4. Wait for status to be **Available** (~5-10 minutes)

> ?? Note the **Endpoint** (e.g., `airelief-db.xxxx.eu-west-1.rds.amazonaws.com`) — this is your DB host.

Your connection string will be:
```
Host=airelief-db.xxxx.eu-west-1.rds.amazonaws.com;Port=5432;Database=aireliefdb;Username=airelief;Password=YOUR_PASSWORD
```

---

### STEP 4 — Create an EFS File System (for Data Protection keys) `[MANUAL — AWS Console]`

1. Go to **EFS Console** ? **Create file system**
2. Name: `airelief-keys`
3. VPC: `airelief-vpc`
4. Click **Create**, then click into the file system
5. Go to **Network** tab ? **Manage** ? ensure mount targets exist in your **private subnets** using the `airelief-ecs-sg` security group
6. Go to **Access points** ? **Create access point**:
   - Root directory path: `/airelief-keys`
   - POSIX user: UID `1000`, GID `1000`
   - Root directory creation permissions: Owner UID `1000`, Owner GID `1000`, Permissions `755`

> ?? Note the **File System ID** (e.g., `fs-0abc123`) and **Access Point ID** (e.g., `fsap-0abc123`).

---

### STEP 5 — Create an ECR Repository & Push Docker Image `[CLI]`

Run these commands from the solution root (the directory containing the `Dockerfile`):

```bash
# Create the ECR repository
aws ecr create-repository --repository-name airelief --region YOUR_REGION

# Authenticate Docker with ECR
aws ecr get-login-password --region YOUR_REGION | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.YOUR_REGION.amazonaws.com

# Build the Docker image
docker build -t airelief .

# Tag it for ECR
docker tag airelief:latest YOUR_ACCOUNT_ID.dkr.ecr.YOUR_REGION.amazonaws.com/airelief:latest

# Push to ECR
docker push YOUR_ACCOUNT_ID.dkr.ecr.YOUR_REGION.amazonaws.com/airelief:latest
```

> Replace `YOUR_ACCOUNT_ID` and `YOUR_REGION` (e.g., `eu-west-1`).

---

### STEP 6 — Create IAM Roles for ECS `[MANUAL — AWS Console]`

#### 6a. ECS Task Execution Role
1. Go to **IAM** ? **Roles** ? **Create role**
2. Trusted entity: **Elastic Container Service** ? **Elastic Container Service Task**
3. Attach policy: `AmazonECSTaskExecutionRolePolicy`
4. Name: `ecsTaskExecutionRole`

#### 6b. ECS Task Role (for EFS access)
1. **Create role** ? Trusted entity: **Elastic Container Service** ? **Elastic Container Service Task**
2. Attach policy: `AmazonElasticFileSystemClientReadWriteAccess`
3. Name: `airelief-task-role`

---

### STEP 7 — Create ECS Cluster `[MANUAL — AWS Console]`

1. Go to **ECS Console** ? **Clusters** ? **Create cluster**
2. Cluster name: `airelief-cluster`
3. Infrastructure: **AWS Fargate** (should be selected by default)
4. Click **Create**

---

### STEP 8 — Create an ALB with Sticky Sessions `[MANUAL — AWS Console]`

#### 8a. Create Target Group
1. Go to **EC2 Console** ? **Target Groups** ? **Create target group**
2. Target type: **IP addresses**
3. Name: `airelief-tg`
4. Protocol: **HTTP**, Port: **8080**
5. VPC: `airelief-vpc`
6. Health check path: `/health`
7. Click **Next** ? **Create target group** (don't register targets — ECS does this)

**Enable sticky sessions:**
1. Select the target group ? **Attributes** tab ? **Edit**
2. **Stickiness**: Enable
3. Type: **Application-based cookie** or **Load balancer generated cookie**
4. Duration: **1 hour** (or match your session timeout of 30 minutes)
5. Save

#### 8b. Create Application Load Balancer
1. Go to **EC2 Console** ? **Load Balancers** ? **Create Load Balancer** ? **Application Load Balancer**
2. Name: `airelief-alb`
3. Scheme: **Internet-facing**
4. IP address type: IPv4
5. VPC: `airelief-vpc`
6. Mappings: Select your **2 public subnets**
7. Security group: `airelief-alb-sg`
8. Listener: HTTP:80 ? Forward to `airelief-tg`
9. Click **Create**

> ?? Note the ALB DNS name (e.g., `airelief-alb-123456.eu-west-1.elb.amazonaws.com`).

#### 8c. (Optional) Add HTTPS listener
1. Request a certificate in **ACM** (AWS Certificate Manager) for your domain
2. Add listener: HTTPS:443 ? Forward to `airelief-tg`, select the ACM certificate
3. Modify HTTP:80 listener to **Redirect to HTTPS:443**

---

### STEP 9 — Create ECS Task Definition `[MANUAL — AWS Console]`

1. Go to **ECS Console** ? **Task definitions** ? **Create new task definition** (JSON tab)
2. Use this template (replace placeholders):

```json
{
  "family": "airelief-task",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::YOUR_ACCOUNT_ID:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::YOUR_ACCOUNT_ID:role/airelief-task-role",
  "containerDefinitions": [
    {
      "name": "airelief",
      "image": "YOUR_ACCOUNT_ID.dkr.ecr.YOUR_REGION.amazonaws.com/airelief:latest",
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ConnectionStrings__IdentityConnection",
          "value": "Host=airelief-db.xxxx.YOUR_REGION.rds.amazonaws.com;Port=5432;Database=aireliefdb;Username=airelief;Password=YOUR_DB_PASSWORD"
        },
        {
          "name": "Smtp__Host",
          "value": "email-smtp.YOUR_REGION.amazonaws.com"
        },
        {
          "name": "Smtp__Port",
          "value": "587"
        },
        {
          "name": "Smtp__Username",
          "value": "YOUR_SES_SMTP_USER"
        },
        {
          "name": "Smtp__Password",
          "value": "YOUR_SES_SMTP_PASSWORD"
        },
        {
          "name": "Smtp__FromAddress",
          "value": "noreply@yourdomain.com"
        }
      ],
      "mountPoints": [
        {
          "sourceVolume": "efs-keys",
          "containerPath": "/app/keys",
          "readOnly": false
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/airelief",
          "awslogs-region": "YOUR_REGION",
          "awslogs-stream-prefix": "ecs",
          "awslogs-create-group": "true"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ],
  "volumes": [
    {
      "name": "efs-keys",
      "efsVolumeConfiguration": {
        "fileSystemId": "fs-YOUR_EFS_ID",
        "transitEncryption": "ENABLED",
        "authorizationConfig": {
          "accessPointId": "fsap-YOUR_ACCESS_POINT_ID",
          "iam": "ENABLED"
        }
      }
    }
  ]
}
```

> ?? **Security tip:** For production, use **AWS Secrets Manager** for the DB password instead of plain-text environment variables. You can reference secrets in the task definition using `"valueFrom": "arn:aws:secretsmanager:..."`.

---

### STEP 10 — Create ECS Service `[MANUAL — AWS Console]`

1. Go to **ECS Console** ? **Clusters** ? `airelief-cluster` ? **Services** ? **Create**
2. Settings:
   - Launch type: **Fargate**
   - Task definition: `airelief-task` (latest revision)
   - Service name: `airelief-service`
   - Desired tasks: **2** (for HA; start with 1 to test)
   - Deployment: **Rolling update**
3. Networking:
   - VPC: `airelief-vpc`
   - Subnets: **Private subnets**
   - Security group: `airelief-ecs-sg`
   - Auto-assign public IP: **DISABLED** (traffic comes through ALB)
4. Load balancing:
   - Type: **Application Load Balancer**
   - Select existing: `airelief-alb`
   - Container: `airelief` : `8080`
   - Target group: `airelief-tg`
5. Auto Scaling (optional):
   - Min tasks: 1
   - Max tasks: 4
   - Scaling policy: Target tracking — Average CPU at 70%
6. Click **Create**

---

### STEP 11 — Configure DNS (Route 53) `[MANUAL — AWS Console]`

1. Go to **Route 53** ? your hosted zone
2. Create an **A record** (or CNAME) for each tenant hostname:
   - `www.airelief.com` ? Alias to ALB
   - `www.aicag.com` ? Alias to ALB
   - `www.aidescanso.com` ? Alias to ALB
3. The `TenantMiddleware` will resolve the correct tenant based on the `Host` header

---

### STEP 12 — Update Tenant HostNames for your domain `[CODE CHANGE — Optional]`

If your production hostnames differ from what's in `appsettings.json`, create/update the environment-specific config. You can override via environment variables in the task definition:

```
Tenants__relief__HostNames__0=www.airelief.com
Tenants__relief__HostNames__1=airelief.com
```

Or update `appsettings.Production.json` before building the Docker image.

---

## Verification Checklist

After deployment, verify:

- [ ] ALB DNS resolves and returns HTTP 200 on `/health`
- [ ] Home page loads correctly at your domain
- [ ] User registration and login work
- [ ] Trial quiz completes successfully (tests session + sticky sessions)
- [ ] Lesson flow works for logged-in users (tests DB-backed state)
- [ ] Admin pages load for admin users
- [ ] Scale to 2+ tasks and repeat the above tests

---

## Cost Estimate (approximate, eu-west-1 pricing)

| Service | Config | ~Monthly Cost |
|---------|--------|--------------|
| ECS Fargate | 2 tasks × 0.5 vCPU / 1 GB | ~$30 |
| RDS PostgreSQL | db.t3.micro (free tier eligible) | $0–$15 |
| ALB | Basic | ~$18 + data |
| EFS | Minimal usage for keys | ~$0.30 |
| NAT Gateway | 1 AZ | ~$33 |
| ECR | Image storage | ~$1 |
| **Total** | | **~$80–100/month** |

> ?? **Cost saving:** Use a single NAT Gateway (not per-AZ) to save ~$33/month. For the lowest cost, you can run a single Fargate task initially.

---

## Updating the Application

To deploy a new version:

```bash
# Build and push new image
docker build -t airelief .
docker tag airelief:latest YOUR_ACCOUNT_ID.dkr.ecr.YOUR_REGION.amazonaws.com/airelief:latest
docker push YOUR_ACCOUNT_ID.dkr.ecr.YOUR_REGION.amazonaws.com/airelief:latest

# Force new deployment (pulls latest image)
aws ecs update-service --cluster airelief-cluster --service airelief-service --force-new-deployment --region YOUR_REGION
```

The rolling deployment will start new tasks, health-check them, then drain old tasks — **zero downtime**.

---

## Troubleshooting

| Issue | Check |
|-------|-------|
| Tasks keep restarting | Check CloudWatch logs at `/ecs/airelief` — likely DB connection string issue |
| 502 Bad Gateway | ECS tasks haven't passed health check yet, or security group doesn't allow ALB ? 8080 |
| Login works then fails | Data Protection keys not shared — verify EFS mount at `/app/keys` |
| Session lost mid-quiz | Sticky sessions not enabled on target group |
| DB migration fails | Ensure RDS security group allows inbound from ECS security group on port 5432 |
