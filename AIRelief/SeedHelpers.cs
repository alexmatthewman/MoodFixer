using Microsoft.AspNetCore.Identity;
using AIRelief.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AIRelief
{
    public static class SeedHelpers
    {
        public static async System.Threading.Tasks.Task EnsureInitialSystemUser(UserManager<IdentityUser> userManager, AIReliefContext context)
        {
            var email    = "alex.matthewman@gmail.com";
            var password = "Ne14txxx!";

            // Ensure Identity user exists
            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                identityUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(identityUser, password);
                if (!result.Succeeded)
                    return;
            }

            // Ensure app-level User row exists with SystemAdmin level
            var appUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (appUser == null)
            {
                context.Users.Add(new User
                {
                    Name        = "System Administrator",
                    Email       = email,
                    AuthLevel   = AuthLevel.SystemAdmin,
                    CreatedDate = System.DateTime.UtcNow
                });
            }
            else if (appUser.AuthLevel != AuthLevel.SystemAdmin)
            {
                appUser.AuthLevel = AuthLevel.SystemAdmin;
            }

            await context.SaveChangesAsync();

            await EnsureSeedQuestions(context);
        }

        private static async System.Threading.Tasks.Task EnsureSeedQuestions(AIReliefContext context)
        {
            if (await context.Questions.AnyAsync())
                return;

            context.Questions.AddRange(
                new Question
                {
                    maintext         = "A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?",
                    image            = "q1.png",
                    explanationtext  = "At first glance, you might think the eraser costs $0.50 since the pencil costs 50 cents more. But, if the pencil costs $0.75 and the eraser costs $0.25, together they add up to $1.00. The first instinct is often to assume the eraser costs more because of how the question is framed, but the correct breakdown is $0.25 for the eraser and $0.75 for the pencil.",
                    explanationimage = "q1x.png",
                    Option1          = "$0.50",
                    Option2          = "$0.25",
                    Option3          = "$0.75",
                    Option4          = "$0.05",
                    CorrectAnswer    = "$0.25",
                    Category         = QuestionCategory.Trial
                },
                new Question
                {
                    maintext         = "In a race, you pass the person in second place. What place are you in now?",
                    image            = "q2.png",
                    explanationtext  = "It's easy to think you're now in 1st place, but if you pass the person in second place, you're now in 2nd, not 1st. The person in 1st is still ahead of you.",
                    explanationimage = "q2x.png",
                    Option1          = "2nd",
                    Option2          = "1st",
                    Option3          = "3rd",
                    Option4          = "4th",
                    CorrectAnswer    = "2nd",
                    Category         = QuestionCategory.Trial
                },
                new Question
                {
                    maintext         = "If a train leaves New York at 10:00 AM and travels at 60 miles per hour, and another train leaves the same station at the same time but travels at 90 miles per hour, how long will it take before the second train catches up to the first?",
                    explanationtext  = "This is a trick question. The second train is faster, but both trains are leaving from the same station at the same time. They'll never catch up because they're already on the same path, just traveling at different speeds.",
                    explanationimage = "q3x.png",
                    Option1          = "1 hour",
                    Option2          = "30 minutes",
                    Option3          = "They will never meet",
                    Option4          = "2 hours",
                    CorrectAnswer    = "They will never meet",
                    Category         = QuestionCategory.Trial
                },
                new Question
                {
                    maintext         = "A car travels 30 miles in 30 minutes. What is the average speed of the car?",
                    image            = "q4.png",
                    explanationtext  = "The car travels 30 miles in 30 minutes, which is the same as 0.5 hours. So, the average speed is 30 miles ÷ 0.5 hours = 60 miles per hour. The trick is in interpreting the time properly and not letting the \"30 minutes\" confuse you.",
                    Option1          = "60 miles per hour",
                    Option2          = "30 miles per hour",
                    Option3          = "15 miles per hour",
                    Option4          = "2 miles per minute",
                    CorrectAnswer    = "60 miles per hour",
                    Category         = QuestionCategory.Trial
                }
            );

            await context.SaveChangesAsync();
        }
    }
}

