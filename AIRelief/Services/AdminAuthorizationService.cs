using AIRelief.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AIRelief.Services
{
    /// <summary>
    /// Service to handle authorization checks for admin access levels.
    /// This provides a centralized, consistent way to validate user permissions.
    /// </summary>
    public class AdminAuthorizationService
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminAuthorizationService(AIReliefContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the application user with their auth level and group info.
        /// Returns null if user is not found or not properly initialized.
        /// </summary>
        public async Task<User> GetAppUserAsync(IdentityUser identityUser)
        {
            if (identityUser == null)
                return null;

            return await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);
        }

        /// <summary>
        /// Validates that user is a Group Admin and belongs to a valid group.
        /// </summary>
        public async Task<bool> IsValidGroupAdminAsync(User appUser)
        {
            if (appUser == null)
                return false;

            // Must have GroupAdmin auth level
            if (appUser.AuthLevel != AuthLevel.GroupAdmin)
                return false;

            // Must be assigned to a group
            if (appUser.GroupId == null)
                return false;

            // Group must exist
            var group = await _context.Groups.FindAsync(appUser.GroupId);
            return group != null;
        }

        /// <summary>
        /// Validates that user is a System Admin.
        /// </summary>
        public bool IsValidSystemAdmin(User appUser)
        {
            if (appUser == null)
                return false;

            return appUser.AuthLevel == AuthLevel.SystemAdmin;
        }

        /// <summary>
        /// Validates user can manage group admins as a System Admin.
        /// Optional groupId can be provided to validate access to specific group.
        /// </summary>
        public async Task<bool> CanManageGroupAdminAsync(User appUser, int? groupId = null)
        {
            // Must be System Admin
            if (!IsValidSystemAdmin(appUser))
                return false;

            // If specific group is provided, verify it exists
            if (groupId.HasValue)
            {
                var group = await _context.Groups.FindAsync(groupId.Value);
                return group != null;
            }

            return true;
        }

        /// <summary>
        /// Validates user can manage users as a Group Admin or System Admin.
        /// For Group Admins: can only manage users in their own group.
        /// For System Admins: can manage any user (optionally filter by group).
        /// </summary>
        public async Task<bool> CanManageUserAsync(User appUser, int? targetGroupId = null)
        {
            if (appUser == null)
                return false;

            // System Admins can manage any user
            if (IsValidSystemAdmin(appUser))
            {
                // If specific group is provided, verify it exists
                if (targetGroupId.HasValue)
                {
                    var group = await _context.Groups.FindAsync(targetGroupId.Value);
                    return group != null;
                }
                return true;
            }

            // Group Admins can only manage users in their own group
            if (appUser.AuthLevel == AuthLevel.GroupAdmin && appUser.GroupId.HasValue)
            {
                // If no target group specified, use their own group
                if (!targetGroupId.HasValue)
                    return true;

                // Can only manage users in their own group
                return targetGroupId.Value == appUser.GroupId.Value;
            }

            return false;
        }

        /// <summary>
        /// Validates that user can manage groups (System Admin only).
        /// </summary>
        public bool CanManageGroups(User appUser)
        {
            return IsValidSystemAdmin(appUser);
        }

        /// <summary>
        /// Validates user can access the group admin dashboard.
        /// </summary>
        public async Task<bool> CanAccessGroupAdminDashboardAsync(User appUser)
        {
            return await IsValidGroupAdminAsync(appUser);
        }

        /// <summary>
        /// Validates user can access System Admin dashboard.
        /// </summary>
        public bool CanAccessSystemAdminDashboard(User appUser)
        {
            return IsValidSystemAdmin(appUser);
        }
    }
}