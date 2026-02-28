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
            bool hasTrial               = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.Trial);
            bool hasCausalReason        = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.CausalReasoning);
            bool hasCognitiveReflection = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.CognitiveReflection);
            bool hasMetacognition          = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.Metacognition);
            bool hasReadingComprehension = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.ReadingComprehension);
            bool hasShortTermMemory         = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.ShortTermMemory);
            bool hasConfidenceCalibration = await context.Questions.AnyAsync(q => q.Category == QuestionCategory.ConfidenceCalibration);

            if (hasTrial && hasCausalReason && hasCognitiveReflection && hasMetacognition && hasReadingComprehension && hasShortTermMemory && hasConfidenceCalibration)
                return;

            if (!hasTrial)
            context.Questions.AddRange(
                new Question
                {
                    MainText         = "A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?",
                    Image            = "q1.png",
                    ExplanationText  = "At first glance, you might think the eraser costs $0.50 since the pencil costs 50 cents more. But, if the pencil costs $0.75 and the eraser costs $0.25, together they add up to $1.00. The first instinct is often to assume the eraser costs more because of how the question is framed, but the correct breakdown is $0.25 for the eraser and $0.75 for the pencil.",
                    ExplanationImage = "q1x.png",
                    Option1          = "$0.50",
                    Option2          = "$0.25",
                    Option3          = "$0.75",
                    Option4          = "$0.05",
                    CorrectAnswer    = "$0.25",
                    Category         = QuestionCategory.Trial
                },
                new Question
                {
                    MainText         = "In a race, you pass the person in second place. What place are you in now?",
                    Image            = "q2.png",
                    ExplanationText  = "It's easy to think you're now in 1st place, but if you pass the person in second place, you're now in 2nd, not 1st. The person in 1st is still ahead of you.",
                    ExplanationImage = "q2x.png",
                    Option1          = "2nd",
                    Option2          = "1st",
                    Option3          = "3rd",
                    Option4          = "4th",
                    CorrectAnswer    = "2nd",
                    Category         = QuestionCategory.Trial
                },
                new Question
                {
                    MainText         = "If a train leaves New York at 10:00 AM and travels at 60 miles per hour, and another train leaves the same station at the same time but travels at 90 miles per hour, how long will it take before the second train catches up to the first?",
                    ExplanationText  = "This is a trick question. The second train is faster, but both trains are leaving from the same station at the same time. They'll never catch up because they're already on the same path, just traveling at different speeds.",
                    ExplanationImage = "q3x.png",
                    Option1          = "1 hour",
                    Option2          = "30 minutes",
                    Option3          = "They will never meet",
                    Option4          = "2 hours",
                    CorrectAnswer    = "They will never meet",
                    Category         = QuestionCategory.Trial
                },
                new Question
                {
                    MainText         = "A car travels 30 miles in 30 minutes. What is the average speed of the car?",
                    Image            = "q4.png",
                    ExplanationText  = "The car travels 30 miles in 30 minutes, which is the same as 0.5 hours. So, the average speed is 30 miles u{00F7} 0.5 hours = 60 miles per hour. The trick is in interpreting the time properly and not letting the \"30 minutes\" confuse you.",
                    Option1          = "60 miles per hour",
                    Option2          = "30 miles per hour",
                    Option3          = "15 miles per hour",
                    Option4          = "2 miles per minute",
                    CorrectAnswer    = "60 miles per hour",
                    Category         = QuestionCategory.Trial
                }
            );

            if (!hasCausalReason)
            context.Questions.AddRange(
                new Question
                {
                    QuestionText     = "Which answer best explains why the bike-sharing program may not be the real cause of fewer accidents?",
                    MainText         = "A small town introduces a bike-sharing program, and over the next year, local traffic accidents decline. Officials suggest that more people cycling reduces car use, improving safety. However, the program coincides with the installation of new traffic lights, a public awareness campaign on road safety, and the repaving of several major roads. Some residents also reported that traffic patterns shifted due to new commuting options. This combination of changes makes it difficult to know which factor truly accounts for the drop in accidents.",
                    Option1          = "Traffic lights, campaigns, and repaved roads could explain the decline",
                    Option2          = "Cyclists may still get into accidents",
                    Option3          = "People might dislike cycling in bad weather",
                    Option4          = "Bicycle ownership is unrelated to traffic accidents",
                    Option5          = "The town's population may have decreased",
                    CorrectAnswer    = "Traffic lights, campaigns, and repaved roads could explain the decline",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Multiple interventions occurred simultaneously, each of which could plausibly influence accident rates. While cycling might contribute, attributing the change solely to the bike-sharing program ignores these other factors.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why the study groups may not be the real cause of better grades?",
                    MainText         = "Researchers observe that students who participate in study groups earn higher grades. They conclude that study groups improve academic performance. Participation is voluntary, and more diligent or highly motivated students are often overrepresented in these groups. Some students also reported that they seek study groups only when preparing for particularly challenging courses. Without data tracking students' prior habits or comparing them to similar students who do not join groups, it is hard to know if study groups are the cause or simply correlated with motivated students.",
                    Option1          = "Students enjoy socializing",
                    Option2          = "More motivated or diligent students may choose to join",
                    Option3          = "Grades are subjective",
                    Option4          = "Some study groups are larger than others",
                    Option5          = "Professors adjust difficulty each semester",
                    CorrectAnswer    = "More motivated or diligent students may choose to join",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "Self-selection into study groups could account for the higher grades. Motivation and prior diligence may explain why students both join groups and perform well, making it unclear whether the study group itself caused the improvement.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best describes why we cannot be sure flexible hours caused the rise in satisfaction?",
                    MainText         = "A company introduces flexible work hours, and employee reports of job satisfaction increase over six months. Management credits the policy for this improvement. At the same time, the company upgraded office amenities, improved team communication practices, and encouraged employees to attend professional development workshops. Employees' job responsibilities also shifted slightly, giving more autonomy to certain teams. With so many concurrent changes, it is difficult to isolate the effect of flexible hours alone.",
                    Option1          = "Flexible hours are the sole cause of higher satisfaction",
                    Option2          = "Upgraded amenities alone caused higher satisfaction",
                    Option3          = "Multiple simultaneous changes make it unclear which factor mattered most",
                    Option4          = "Satisfaction always increases after policy changes",
                    Option5          = "Employee reports are irrelevant",
                    CorrectAnswer    = "Multiple simultaneous changes make it unclear which factor mattered most",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Several changes occurred at once, any of which could influence satisfaction. A careful interpretation acknowledges that flexible hours may contribute, but the data do not show that they are the only cause.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why the gardens may not be the real cause of lower crime?",
                    MainText         = "A city observes that neighbourhoods with more community gardens have lower crime rates. City planners suggest that gardens reduce criminal activity by fostering social cohesion and improving neighbourhood engagement. However, these same neighbourhoods tend to have more active community organisations, higher average income, and lower rates of vacant properties. Without accounting for these other characteristics, it is difficult to know whether the gardens themselves reduce crime or if they are simply more common in safer, more organised neighbourhoods.",
                    Option1          = "Gardens take up space",
                    Option2          = "Crime varies randomly",
                    Option3          = "Plants require maintenance",
                    Option4          = "Higher income and active associations may explain lower crime",
                    Option5          = "Residents might report crime differently",
                    CorrectAnswer    = "Higher income and active associations may explain lower crime",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Other neighbourhood features, such as income and social organisation, could be driving the lower crime rates. Gardens may contribute, but the observed association may largely reflect these other factors.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why drinking tea may not be the real cause of fewer colds?",
                    MainText         = "A health magazine reports that people who drink herbal tea daily report fewer colds. No longitudinal data are available, and survey respondents are self-selected, often including individuals who already follow other healthy behaviours such as exercising, taking vitamins, or getting sufficient sleep. Without tracking these factors, it is unclear whether tea itself has any protective effect, or whether people who drink tea are simply more likely to engage in other behaviours that reduce illness. The magazine promotes the tea as a direct remedy for colds.",
                    Option1          = "Herbal tea tastes good",
                    Option2          = "Survey participation is voluntary",
                    Option3          = "Colds are unpredictable",
                    Option4          = "Tea is expensive",
                    Option5          = "Health-conscious individuals may both drink tea and take other preventive measures",
                    CorrectAnswer    = "Health-conscious individuals may both drink tea and take other preventive measures",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "People who already practise healthy habits may also drink tea, creating a confounding factor. The association between tea consumption and fewer colds does not necessarily reflect a causal effect.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why the mentorship programme may not be the real cause of greater confidence?",
                    MainText         = "A small company introduces a mentorship programme. Six months later, employees who participate report higher confidence in handling tasks. Management attributes this change to the mentorship programme. However, the employees selected for mentorship were among the most ambitious and already had strong performance records. In addition, some participants had recently completed external training courses. Without accounting for these pre-existing differences, it is difficult to conclude that the mentorship programme alone caused the improvement in confidence.",
                    Option1          = "Mentors are paid",
                    Option2          = "Selection bias based on ambition and past performance",
                    Option3          = "Confidence cannot be measured",
                    Option4          = "Only a few employees participated",
                    Option5          = "Tasks became easier",
                    CorrectAnswer    = "Selection bias based on ambition and past performance",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "Employees chosen for mentorship may have improved confidence regardless of the programme. Selection bias complicates the causal interpretation.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why the longer hours may not be the real cause of higher library attendance?",
                    MainText         = "After a local library extends evening hours, attendance rises noticeably over several months. Library officials conclude that longer hours directly caused the increase. However, the library also ran a citywide literacy campaign, introduced new reading programmes for teens, and improved the online catalogue at the same time. Some residents mentioned that they were motivated by both the programmes and the extended hours. The combination of multiple interventions makes it unclear which factor drove the increase in attendance.",
                    Option1          = "Promotional campaigns and new programmes may have increased attendance",
                    Option2          = "Evening hours might be inconvenient",
                    Option3          = "Libraries cannot influence reading habits",
                    Option4          = "Attendance is always seasonal",
                    Option5          = "Some residents do not visit libraries",
                    CorrectAnswer    = "Promotional campaigns and new programmes may have increased attendance",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Multiple simultaneous changes could explain the rise in attendance. The extended hours may contribute, but they cannot be identified as the sole cause.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why walking may not be the real cause of lower stress?",
                    MainText         = "A study finds that employees who take short afternoon walks report lower stress levels than those who do not. Researchers suggest walking reduces stress. However, employees who take walks tend to have flexible schedules, supportive managers, and less demanding workloads. Some also engage in mindfulness exercises during breaks. These other factors could plausibly explain why stress levels are lower, making it difficult to isolate the effect of walking alone.",
                    Option1          = "Walking has no physical benefits",
                    Option2          = "Employees may exaggerate their stress",
                    Option3          = "Stress is subjective",
                    Option4          = "Other workplace factors, like flexible schedules or support, could account for lower stress",
                    Option5          = "Walks only occur on sunny days",
                    CorrectAnswer    = "Other workplace factors, like flexible schedules or support, could account for lower stress",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Lower stress may be due to multiple aspects of the work environment, not just walking. The evidence does not clearly isolate walking as the causal factor.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best describes what we can reasonably conclude about the rise in productivity?",
                    MainText         = "A tech company implements a new task management system. During the same quarter, productivity metrics improve noticeably. However, several teams also received training on time management, and managers began using new performance tracking tools. Some employees reported that these concurrent changes made it easier to complete tasks efficiently. Because multiple interventions occurred simultaneously, it is difficult to determine whether the task management system itself caused the improvement.",
                    Option1          = "The new system caused all productivity gains",
                    Option2          = "Training alone explains productivity",
                    Option3          = "Both the system and training could have contributed",
                    Option4          = "Productivity is unrelated to the system",
                    Option5          = "Metrics are meaningless",
                    CorrectAnswer    = "Both the system and training could have contributed",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Multiple changes occurred at once, any of which could influence productivity. A careful interpretation acknowledges that the system may contribute, but it is likely not the only factor.",
                    Category         = QuestionCategory.CausalReasoning
                },
                new Question
                {
                    QuestionText     = "Which answer best explains why the festival may not be the real cause of higher hotel bookings?",
                    MainText         = "A city observes that during months when it hosts a music festival, local hotel bookings increase substantially. Officials suggest the festival drives tourism. However, these months also coincide with school holidays, warmer weather, and a regional food festival. Some residents report that visitors come for multiple reasons, not just the music festival. Without controlling for these other seasonal factors, it is difficult to know how much of the increase in bookings is truly caused by the festival itself.",
                    Option1          = "Hotels are more attractive in summer",
                    Option2          = "Local restaurants may also attract visitors",
                    Option3          = "Festivals are loud",
                    Option4          = "Bookings depend on online platforms",
                    Option5          = "Tourism may rise due to holidays, weather, or other events, independent of the festival",
                    CorrectAnswer    = "Tourism may rise due to holidays, weather, or other events, independent of the festival",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "Other seasonal factors and events could explain the higher bookings. The festival may contribute, but the data do not clearly isolate its effect.",
                    Category         = QuestionCategory.CausalReasoning
                }
            );

            if (!hasCognitiveReflection)
            context.Questions.AddRange(
                new Question
                {
                    QuestionText     = "How much does the pen cost?",
                    MainText         = "At a stationery store, a notebook costs exactly $5 more than a pen. A customer purchases one pen and one notebook and pays $5.50 in total. Another shopper wonders how much the pen costs, thinking about how the difference in price relates to the total paid and whether a simple estimate will suffice.",
                    Option1          = "$0.25",
                    Option2          = "$0.50",
                    Option3          = "$0.75",
                    Option4          = "$1.00",
                    Option5          = "Cannot be determined",
                    CorrectAnswer    = "$0.25",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "The intuitive answer is $0.50, which seems reasonable, but then the notebook would cost $5.50, totaling $6, not $5.50. If the pen costs $0.25, the notebook is $5.25, for a total of $5.50. The error comes from assuming a rough estimate satisfies both conditions, rather than carefully checking the constraints.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "How long will 100 machines take to produce 100 devices?",
                    MainText         = "A factory has five machines, and together they produce five devices in five minutes. The production manager wonders whether increasing the number of machines will proportionally reduce the time for larger orders. She tries to reason about whether each additional machine shortens the time per device or just increases overall output.",
                    Option1          = "1 minute",
                    Option2          = "5 minutes",
                    Option3          = "20 minutes",
                    Option4          = "100 minutes",
                    Option5          = "Cannot be determined",
                    CorrectAnswer    = "5 minutes",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "Each machine produces one device in five minutes. Intuition might suggest more machines dramatically reduce time, but since each device still requires five minutes per machine, 100 machines produce 100 devices in the same five minutes. The mistake comes from assuming scaling machines changes the time per item, rather than total throughput.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "On which day was the pond half covered?",
                    MainText         = "A pond in a park has algae that double in size every day. Park staff note that it takes 30 days for the algae to completely cover the pond. Visitors speculate about when the pond was half covered, often assuming growth is linear rather than exponential, without carefully considering the doubling pattern.",
                    Option1          = "Day 15",
                    Option2          = "Day 20",
                    Option3          = "Day 29",
                    Option4          = "Day 25",
                    Option5          = "Day 30",
                    CorrectAnswer    = "Day 29",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Full coverage occurs on day 30. The pond was half covered the day before because the algae double daily. Linear reasoning would suggest day 15 or 20, but exponential growth dramatically shifts the halfway point. The error comes from applying a linear intuition to a doubling process.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "What is the average speed for the whole trip?",
                    MainText         = "A car travels 60 km from City A to City B at 60 km/h. On the return trip, along the same 60 km route, it travels at 40 km/h. Some assume the average speed for the round trip is the midpoint of the two speeds, without considering how time spent at each speed affects the overall average.",
                    Option1          = "50 km/h",
                    Option2          = "45 km/h",
                    Option3          = "40 km/h",
                    Option4          = "48 km/h",
                    Option5          = "Cannot be determined",
                    CorrectAnswer    = "48 km/h",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Total distance = 120 km. The first leg takes 1 hour; the return leg takes 1.5 hours. Average speed = 120 divided by 2.5 = 48 km/h. The intuitive midpoint of 60 and 40 km/h overestimates the speed because the slower leg takes longer. Average speed is weighted by time, not the arithmetic mean.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "What must be true when the base of the ladder is moved further from the wall?",
                    MainText         = "A ladder leans against a wall. The base is moved one meter farther from the wall. Observers wonder what must be true about how high the ladder reaches or how the angle changes, while the ladder's length remains constant.",
                    Option1          = "The ladder is now shorter",
                    Option2          = "The ladder angle increases",
                    Option3          = "The wall height changes",
                    Option4          = "Nothing definite follows",
                    Option5          = "The ladder touches the wall at a lower point",
                    CorrectAnswer    = "The ladder touches the wall at a lower point",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "Moving the base outward reduces the vertical height the ladder reaches. Intuition might focus on ladder length or angle, but only the height is guaranteed to change. Geometry dictates this outcome, which is often overlooked when thinking quickly.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "What is the cyclist's average speed for the whole trip?",
                    MainText         = "A cyclist rides for one hour at 20 km/h and then one hour at 30 km/h along the same route. She wonders about her average speed, questioning whether it can be calculated simply from the two speeds or requires a more careful calculation.",
                    Option1          = "22 km/h",
                    Option2          = "24 km/h",
                    Option3          = "25 km/h",
                    Option4          = "26 km/h",
                    Option5          = "Cannot be determined",
                    CorrectAnswer    = "25 km/h",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Total distance = 20 + 30 = 50 km; total time = 2 hours. Average speed = 50 divided by 2 = 25 km/h. Here, the intuitive midpoint of 20 and 30 km/h is correct because time at each speed is equal. Overthinking might suggest a complex formula, but a simple weighted average works.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "What does a positive test result most likely mean for this patient?",
                    MainText         = "A rare disease affects 1 in 1,000 people. A test detects the disease 99% of the time but can give false positives. A patient tests positive, and a doctor wonders what the result indicates about their actual likelihood of having the disease.",
                    Option1          = "Most positive results will be false",
                    Option2          = "A positive result almost certainly means the person has the disease",
                    Option3          = "The test is unreliable",
                    Option4          = "The disease is common",
                    Option5          = "The test is useless",
                    CorrectAnswer    = "Most positive results will be false",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "The disease is very rare, so false positives outnumber true positives. Quick intuition assumes a positive test is almost certain evidence of disease, but the base rate dominates, and most positive results are actually false.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "After the price rise and then the discount, how does the final price compare to the original?",
                    MainText         = "A store raises the price of an item by 20% one month, then offers a 20% discount the next month. Shoppers wonder whether the final price is higher, lower, or the same as the original. Many initially assume the increase and discount cancel each other.",
                    Option1          = "The same",
                    Option2          = "Higher",
                    Option3          = "Exactly 20% lower",
                    Option4          = "Impossible to determine",
                    Option5          = "Lower",
                    CorrectAnswer    = "Higher",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The 20% discount applies to the already increased price, so the final price is slightly higher than the original. Quick intuition assumes additive cancellation, but percentages multiply, which subtly changes the total.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "Across ten draws with replacement, which outcome is most likely?",
                    MainText         = "A bag contains nine white balls and one black ball. A ball is drawn, replaced, and the process repeated ten times. People try to guess the most likely outcome, often underestimating the probability of seeing zero black balls in repeated low-probability events.",
                    Option1          = "Ten black balls drawn",
                    Option2          = "Five black balls drawn",
                    Option3          = "All outcomes equally likely",
                    Option4          = "No black balls drawn",
                    Option5          = "Exactly one black ball drawn",
                    CorrectAnswer    = "No black balls drawn",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Each draw has a 10% chance of a black ball. Over 10 independent draws, no black balls is more likely than exactly one. Intuition expects the rare event to occur at least once, but repeated low-probability events often do not happen at all.",
                    Category         = QuestionCategory.CognitiveReflection
                },
                new Question
                {
                    QuestionText     = "What is the total number of people who will attend?",
                    MainText         = "Emma is starting a small book club with her two closest friends, Sophie and Liam, who know each other well. Sophie plans to bring three people she often discusses books with, and Liam says he will bring two friends who enjoy reading. The group is small and close-knit, with everyone connected socially in the city. Emma tries to figure out how many people will attend, thinking through the list of attendees.",
                    Option1          = "6",
                    Option2          = "8",
                    Option3          = "5",
                    Option4          = "Cannot be determined",
                    Option5          = "7",
                    CorrectAnswer    = "Cannot be determined",
                    BestAnswers      = new[] { 4, 5 },
                    ExplanationText  = "Simply summing the attendees gives 7, which is intuitive. However, because the group is small and socially connected, some guests could be the same people. Quick thinking assumes all invited guests are distinct, but reflection shows that the exact total cannot be determined without knowing whether any guests overlap.",
                    Category         = QuestionCategory.CognitiveReflection
                }
            );

            if (!hasMetacognition)
            context.Questions.AddRange(
                new Question
                {
                    QuestionText     = "Which response best reflects careful thinking about the manager's argument?",
                    MainText         = "A manager is discussing the company's remote work policy during a team meeting. She argues that employees should not be allowed to work from home more than one day a week. She explains that when people are not physically present in the office, collaboration suffers, communication slows, and projects take longer to complete. She notes that in-person interactions often generate spontaneous ideas and faster decision-making. Based on this, she concludes that limiting remote work is necessary to maintain productivity. She does not provide evidence about actual productivity metrics or other ways collaboration could occur remotely.",
                    Option1          = "Considering whether effective collaboration could happen even when some work is done remotely",
                    Option2          = "Accepting her conclusion because office presence seems intuitively important for collaboration",
                    Option3          = "Assuming that all employees prefer working from home",
                    Option4          = "Trusting the manager's opinion because she has experience managing teams",
                    Option5          = "Believing that reducing remote work will automatically increase productivity",
                    CorrectAnswer    = "Considering whether effective collaboration could happen even when some work is done remotely",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "The manager assumes that collaboration only works well in person, but we do not know if remote work could allow collaboration in other ways. Carefully evaluating her argument means noticing this hidden assumption and thinking: are there other ways to collaborate effectively, even if some work is remote? Options A, D, and E rely on intuition or authority rather than reasoning, and C is unrelated to the argument's logic.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "Why might Jordan's conclusion deserve more scrutiny?",
                    MainText         = "Jordan recently read several widely shared news stories about major data breaches at large technology companies. The stories were vivid, detailed, and easy to remember. Afterward, Jordan said that big tech companies clearly cannot be trusted with user data, and that smaller companies are probably much safer. He did not check statistics about how frequently breaches occur, and he did not consider whether the stories were representative of all companies.",
                    Option1          = "He generalised from a few memorable examples rather than evaluating how common such events are",
                    Option2          = "He deliberately sought information that confirmed his prior beliefs",
                    Option3          = "He assumed that small companies are inherently more secure",
                    Option4          = "He relied on the first number he saw in each story",
                    Option5          = "He decided based on emotional impact rather than reasoning",
                    CorrectAnswer    = "He generalised from a few memorable examples rather than evaluating how common such events are",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Jordan's reasoning is influenced by how vivid and memorable the news stories are, not by evidence about how typical breaches actually are. Carefully reflecting here means recognising that just because something is memorable does not make it representative. The other options are tempting shortcuts or related biases, but the key issue is overgeneralisation from memorable examples.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What is the most careful conclusion Person A can draw from what they know?",
                    MainText         = "A card is randomly drawn from a standard deck, and two people receive different pieces of information. Person A is told the card is red. Person B is told the card is a heart. Person A then hears Person B say, 'I know exactly which card it is.' Person A has no other information. Person A tries to figure out what, if anything, can be concluded about the situation based on what is known.",
                    Option1          = "Person B must be mistaken, since there is not enough information",
                    Option2          = "Person B has information beyond what Person A knows",
                    Option3          = "The card is the Ace of Hearts",
                    Option4          = "The card is a face card",
                    Option5          = "The card is not a heart",
                    CorrectAnswer    = "Person B has information beyond what Person A knows",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "Person A cannot determine the exact card because multiple red cards exist. The only safe conclusion is that Person B must have information that allows them to identify it. Carefully reasoning here involves focusing on what you actually know versus what you assume. Options A, C, D, and E jump to conclusions based on limited knowledge.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What should careful thinkers question about the founder's reasoning?",
                    MainText         = "A startup founder is talking with potential investors about a new product. He says he is very confident it will succeed because everyone he has spoken to so far loves the idea. He mentions conversations with friends, early users, and a few industry contacts. Based on this feedback, he concludes that the product has strong market demand. He does not systematically sample potential customers, nor does he check for biases in whom he asked.",
                    Option1          = "Confidence guarantees that the product will succeed",
                    Option2          = "Liking an idea always leads people to buy it",
                    Option3          = "Decisions should never be made early in a startup",
                    Option4          = "The feedback may come from a limited or unrepresentative group",
                    Option5          = "Market success is unrelated to customer interest",
                    CorrectAnswer    = "The feedback may come from a limited or unrepresentative group",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "The founder's conclusion relies heavily on a small, possibly biased sample. Careful reflection means questioning whether the evidence truly represents the broader market. The other options are either exaggerations or irrelevant reasoning. Learners are encouraged to look beyond confidence and anecdotes to evaluate the quality of evidence.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What would be the most careful next step for Maya to take?",
                    MainText         = "After completing a difficult logic puzzle during a problem-solving workshop, Maya feels very confident that her answer is correct. The puzzle reminded her of another one she solved quickly a few weeks earlier, and she remembers having the same strong feeling of certainty then. That earlier answer turned out to be right, which reinforces her confidence now. As she reviewed her work, Maya noticed that one step felt a bit rushed, but she decided not to revisit it, assuming it was probably fine. Later, during group discussion, she hears other participants describe alternative approaches she had not considered.",
                    Option1          = "Reminding herself that the puzzle felt familiar because she had solved similar ones before",
                    Option2          = "Trusting her intuition, since it has worked well for her in the past",
                    Option3          = "Noting that logic puzzles often have only one reasonable solution",
                    Option4          = "Assuming that any small mistake would have been obvious",
                    Option5          = "Rechecking the step she rushed through, even though her confidence feels high",
                    CorrectAnswer    = "Rechecking the step she rushed through, even though her confidence feels high",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "Maya's confidence is influenced by familiarity and past success, which can be misleading. Careful thinking involves checking each part of the reasoning rather than relying on intuition alone. Options A, B, C, and D may feel tempting because they appeal to past experience or perceived obviousness, but they do not verify the current solution.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What does careful thinking reveal about the student's explanation?",
                    MainText         = "After grades are released for a challenging exam, a student explains a classmate's strong performance by saying the classmate is naturally intelligent. When asked what she means, the student points out that the classmate often grasps new material quickly, answers questions correctly in class, and asks thoughtful questions during discussions. Based on these observations, she feels confident that intelligence explains the exam result. She does not consider other factors, such as preparation or familiarity with the material.",
                    Option1          = "Intelligence alone is enough to account for the exam result",
                    Option2          = "The observations cited may describe the outcome rather than explain it",
                    Option3          = "Participation in class reliably predicts exam success",
                    Option4          = "Preparation and effort should be ignored when assessing performance",
                    Option5          = "Intelligence should be inferred only from observable behaviours",
                    CorrectAnswer    = "The observations cited may describe the outcome rather than explain it",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The student may be mistaking description for explanation. Just because someone demonstrates certain behaviours does not mean those behaviours caused the exam outcome. Careful thinking involves asking: does this evidence genuinely explain the result, or am I just restating what I observed? Options A, C, D, and E either overstate the evidence or ignore other factors.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What is the most careful interpretation of the colleague's behaviour?",
                    MainText         = "During a project meeting, Alex notices that a colleague who usually contributes ideas is unusually quiet. The colleague listens attentively but does not speak, even when directly invited to comment. Alex observes that the colleague takes notes and nods occasionally but never volunteers suggestions. After the meeting, Alex concludes that the colleague must be unhappy with the team's current direction. He considers mentioning his impression to other team members but worries he might be misreading the situation.",
                    Option1          = "The colleague is likely unhappy, since silence often signals disengagement",
                    Option2          = "The colleague's reaction probably reflects broader dissatisfaction with the project",
                    Option3          = "There are several possible explanations, and it is unclear which one applies here",
                    Option4          = "The colleague should have spoken up if there was a problem",
                    Option5          = "The team should reconsider its direction",
                    CorrectAnswer    = "There are several possible explanations, and it is unclear which one applies here",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Alex has limited information. Silence can have many meanings such as concentration, shyness, or distraction, so the safest conclusion is to acknowledge uncertainty. Options A and B jump to conclusions, while D and E assume responsibility lies elsewhere. Careful thinking involves resisting the urge to over-interpret and remaining open to multiple possibilities.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What best explains why the team members' memories differ from the written record?",
                    MainText         = "After a long-term project fails to meet its goals, several team members say they suspected from the beginning that the plan would not work. They cite early warning signs and say the outcome now seems obvious. However, meeting notes and emails from the start of the project show that most of the same people expressed cautious optimism and actively supported the plan. The team reflects on why their memories of initial doubts differ from the documented evidence.",
                    Option1          = "Team members are accurately recalling their initial doubts",
                    Option2          = "The project failed because the team ignored early warnings",
                    Option3          = "Only the team leader could have anticipated the failure",
                    Option4          = "Team members are reshaping their memories to match the known outcome",
                    Option5          = "Early signs were irrelevant to the outcome",
                    CorrectAnswer    = "Team members are reshaping their memories to match the known outcome",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "People often remember the past in a way that aligns with what actually happened, a phenomenon called hindsight bias. Recognising this helps learners understand that memory is fallible and that our sense of knowing it all along may be misleading. Options A, B, C, and E fail to consider how perception changes after the fact.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What should Lena do to think more carefully about the article's claim?",
                    MainText         = "Lena reads an online article making a confident claim about nutrition and health. The author writes clearly, uses vivid personal anecdotes, and explains the idea in a way that feels intuitive and coherent. Lena notices that the argument fits well with things she has heard before, and she finds it persuasive. However, the article does not cite scientific studies or independent sources, and Lena wonders whether the conclusions are supported beyond the author's experience.",
                    Option1          = "Accepting the claim because it aligns with prior beliefs",
                    Option2          = "Considering whether the explanation accounts for alternative viewpoints",
                    Option3          = "Assuming that the topic is controversial and therefore false",
                    Option4          = "Judging the claim based solely on clarity and persuasiveness",
                    Option5          = "Checking whether the author provides independent evidence beyond personal experience",
                    CorrectAnswer    = "Checking whether the author provides independent evidence beyond personal experience",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "Even persuasive or familiar explanations can be misleading if they lack independent evidence. Careful thinkers focus on whether the claim is supported, not just whether it feels intuitive or aligns with prior beliefs. Options A, C, and D are shortcuts, and B is helpful but incomplete without checking actual evidence.",
                    Category         = QuestionCategory.Metacognition
                },
                new Question
                {
                    QuestionText     = "What is the most careful way to interpret two studies that disagree?",
                    MainText         = "A reader encounters two research studies examining the same psychological effect. The first reports a small but consistent effect across multiple experiments. The second, using a different sample and method, finds no clear effect. The reader feels frustrated and concludes that the effect probably does not exist. Later, he reads commentary suggesting that the effect may vary under certain conditions that neither study fully explores. He is unsure which conclusion is most reasonable.",
                    Option1          = "The effect should be dismissed until all studies agree",
                    Option2          = "The disagreement shows that the research is unreliable",
                    Option3          = "The results suggest the effect may depend on conditions that are not yet fully understood",
                    Option4          = "The study showing no effect should be trusted more",
                    Option5          = "Small effects are not worth investigating",
                    CorrectAnswer    = "The results suggest the effect may depend on conditions that are not yet fully understood",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Conflicting results do not automatically mean the effect is false. A careful approach is to acknowledge uncertainty and consider that the effect may depend on specific conditions. Options A, B, D, and E force a conclusion too quickly or overgeneralise. This encourages learners to stay open to nuance and match confidence to the strength of the evidence.",
                    Category         = QuestionCategory.Metacognition
                }
            );


            if (!hasReadingComprehension)
            context.Questions.AddRange(
                new Question
                {
                    QuestionText     = "What is the committee's underlying attitude toward the proposal?",
                    MainText         = "In its public announcement, the committee described the proposal as 'innovative, timely, and worthy of serious consideration.' Shortly afterward, however, an internal report struck a more cautious tone. It called for 'further clarification on key assumptions' and recommended delaying implementation until additional data could be gathered. The report did not say what data would be needed or when the proposal might be reconsidered, noting only that moving too quickly could lead to 'unintended complications,' even while acknowledging the proposal's potential benefits.",
                    Option1          = "The committee is hesitant to approve the proposal and appears to be slowing the process",
                    Option2          = "The committee is enthusiastic and expects to move forward once small issues are addressed",
                    Option3          = "The committee is genuinely undecided and waiting for outside input",
                    Option4          = "The committee is generally supportive but wants to refine the proposal first",
                    Option5          = "The committee does not yet have enough information to form any opinion",
                    CorrectAnswer    = "The committee is hesitant to approve the proposal and appears to be slowing the process",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Despite public praise, the internal report gives no clear next steps and leaves the timeline open-ended. The emphasis on caution and convenience makes hesitation the best fit, even though Option D is a reasonable alternative.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "Which assumption most supports the author's argument?",
                    MainText         = "The author openly admits that the earliest studies in the field relied on small samples and had clear methodological limits. Even so, she argues that their conclusions deserve some confidence. Her reasoning is that later research — using larger samples and stronger methods — arrived at similar results, despite focusing on different populations and settings.",
                    Option1          = "Better methods usually lead to different conclusions",
                    Option2          = "When results hold up across different studies, they are more likely to be reliable",
                    Option3          = "Differences in population rarely matter in research",
                    Option4          = "Early researchers correctly predicted later findings",
                    Option5          = "The later studies were designed to confirm the earlier ones",
                    CorrectAnswer    = "When results hold up across different studies, they are more likely to be reliable",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The author's case rests entirely on cross-study consistency. Option C may seem appealing but goes further than the passage requires — the argument depends on consistency, not on populations being irrelevant.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "Why does the policy most likely remain in place?",
                    MainText         = "When the policy was first introduced, it was clearly described as a temporary response to unusual circumstances, with repeated assurances that it would be withdrawn once conditions returned to normal. After that point, however, the policy was extended several times. Each extension was explained as a matter of administrative convenience rather than urgent need, and over time, official statements mentioned the policy's temporary status less and less.",
                    Option1          = "The policy turned out to be more effective than expected",
                    Option2          = "The policy addresses problems that were not anticipated at the start",
                    Option3          = "Keeping the policy is easier than undoing it",
                    Option4          = "The policy enjoys strong public support",
                    Option5          = "The policy was never truly meant to be temporary",
                    CorrectAnswer    = "Keeping the policy is easier than undoing it",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "The passage emphasises administrative convenience and the gradual dropping of 'temporary' language. The other options may be possible in theory but have no supporting evidence in the passage.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "How does the reviewer rate the film overall?",
                    MainText         = "The reviewer repeatedly praises the film's visuals, technical precision, and attention to detail, calling these elements 'consistently impressive.' At the same time, the review returns to the film's lack of a clear or emotionally engaging story, suggesting that strong style alone is not enough to hold the viewer's interest.",
                    Option1          = "The film succeeds mainly because of its technical strengths",
                    Option2          = "The film's storytelling ambition outweighs its technical flaws",
                    Option3          = "The reviewer clearly prefers style over narrative",
                    Option4          = "The film's visual appeal helps, but does not fully make up for its weaknesses",
                    Option5          = "The reviewer sees little value in the film overall",
                    CorrectAnswer    = "The film's visual appeal helps, but does not fully make up for its weaknesses",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "The reviewer appreciates the craftsmanship but ultimately suggests it cannot compensate for the missing narrative depth — a balanced but reserved judgment.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What concern does the passage mainly raise about the platform?",
                    MainText         = "Supporters of the new platform often say it expands access to information by making it easier to publish and discover content. Critics respond that, in practice, the platform's recommendation systems play a major role in shaping what users actually see. These systems tend to promote content that aligns with existing preferences, which may limit exposure to genuinely new or challenging ideas.",
                    Option1          = "The platform deliberately restricts information",
                    Option2          = "Users prefer familiar content over new ideas",
                    Option3          = "Open platforms inevitably reduce content quality",
                    Option4          = "Critics underestimate the benefits of personalisation",
                    Option5          = "Algorithms influence user experience more than people often realise",
                    CorrectAnswer    = "Algorithms influence user experience more than people often realise",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "The concern is structural — the way recommendation systems shape exposure — rather than a matter of user psychology or deliberate restriction.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What does the passage most strongly imply about the council's approach?",
                    MainText         = "Despite the city council's repeated assurances that zoning changes would benefit the community, several residents expressed concern that the revisions would disproportionately favour a few large developers. The council emphasised that public input would still be considered, but no clear timelines were provided, leaving many feeling uncertain. Some residents worried that the public meetings would be largely symbolic, while others held out hope that their opinions might genuinely influence the outcome. Overall, the situation reflected a tension between official assurances and community scepticism.",
                    Option1          = "They are proceeding cautiously while maintaining appearances",
                    Option2          = "They are actively favouring large developers",
                    Option3          = "They are ignoring public opinion entirely",
                    Option4          = "They have already made binding decisions",
                    Option5          = "They are unsure about the long-term effects of the revisions",
                    CorrectAnswer    = "They are proceeding cautiously while maintaining appearances",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "The contrast between reassurances and vague timelines suggests caution combined with a concern for appearances. Option E is a reasonable read but B better captures the subtle nuance of the passage.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "Which conclusion aligns best with the passage about meditation?",
                    MainText         = "A recent study found that participants who engaged in short daily meditation sessions reported slightly higher concentration levels than those who did not. However, the observed difference was smaller than the researchers had initially anticipated. Several participants noted that consistency seemed more important than the length of each session, suggesting that forming a habit mattered more than occasional long sittings. The study highlights that small, repeated actions may have more impact on attention than sporadic effort.",
                    Option1          = "Regular practice is more influential than session length",
                    Option2          = "Meditation has no measurable effect",
                    Option3          = "Longer meditation sessions produce significantly better results",
                    Option4          = "Participants exaggerated their improvements",
                    Option5          = "The researchers' expectations were completely wrong",
                    CorrectAnswer    = "Regular practice is more influential than session length",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Consistency emerges as the key factor in the passage. The other options either overstate or misrepresent what the study found.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "How would you best describe the overall reception of the new menu?",
                    MainText         = "Although the restaurant received mixed reviews for its new menu, several food critics highlighted that the chef's creativity remained evident. Some dishes, they noted, lacked balance, while others displayed inventive combinations that delighted adventurous diners. Patrons' reactions were equally varied: some embraced the novelty and originality, while others felt the flavours were too unconventional. The overall reception revealed both appreciation for creativity and concern over execution.",
                    Option1          = "The chef is failing completely",
                    Option2          = "Patrons and critics are uniformly impressed",
                    Option3          = "Creativity is recognised, but opinions on execution vary",
                    Option4          = "Novel flavours are universally disliked",
                    Option5          = "The menu is safer than previous offerings",
                    CorrectAnswer    = "Creativity is recognised, but opinions on execution vary",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Both critics and patrons acknowledge creativity, but opinions are mixed on execution. Options A and D ignore the nuance present in the passage.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What is the main point about bee population recovery?",
                    MainText         = "After decades of decline, local bee populations have shown signs of recovery in areas where pesticide use has been strictly controlled and native flowering plants reintroduced. Researchers note, however, that the recovery is uneven, with some regions seeing more robust gains than others. Additionally, sudden environmental changes, such as extreme weather events or disease outbreaks, remain potential threats. The passage emphasises both progress and ongoing challenges for bee conservation.",
                    Option1          = "Pesticides are the only factor affecting bees",
                    Option2          = "Bee populations are completely stable now",
                    Option3          = "Recovery is inevitable everywhere",
                    Option4          = "Environmental management can support bee recovery, but results vary",
                    Option5          = "Native plants harm bees",
                    CorrectAnswer    = "Environmental management can support bee recovery, but results vary",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "The passage focuses on interventions and caution about uneven results. All other options misstate or overstate the passage's message.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What does the passage conclude about automated translation tools?",
                    MainText         = "A recent technology review highlights that while automated translation tools have improved dramatically over the past few years, they still struggle with idioms, humour, and culturally specific references. Users who rely solely on these tools may misinterpret subtle meaning or fail to appreciate context. The review suggests that while these tools are helpful for general understanding, human oversight remains important for accurate interpretation. Overall, the passage balances praise for technological progress with caution about limitations.",
                    Option1          = "Translation tools are flawless",
                    Option2          = "Users should never use automated translations",
                    Option3          = "Cultural context is unimportant",
                    Option4          = "Idioms are irrelevant",
                    Option5          = "Machine translation is useful but imperfect, especially with nuance",
                    CorrectAnswer    = "Machine translation is useful but imperfect, especially with nuance",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "The passage emphasises usefulness alongside clear limitations, particularly regarding subtlety and cultural context.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "How would you best describe the reviewer's tone?",
                    MainText         = "The reviewer remarked that the speaker 'offered a generous number of statistics' while 'failing to connect them to any clear argument,' leaving the audience both impressed and bewildered. While the statistics demonstrated preparation and diligence, the lack of narrative made it difficult to follow the speaker's reasoning. The tone of the review conveys mild amusement mixed with critique, reflecting both appreciation and concern.",
                    Option1          = "Mildly amused and critical",
                    Option2          = "Fully impressed",
                    Option3          = "Outright hostile",
                    Option4          = "Indifferent",
                    Option5          = "Confused without judgement",
                    CorrectAnswer    = "Mildly amused and critical",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Words like 'impressed and bewildered' and the description of 'mild amusement mixed with critique' point clearly to gentle, amused criticism rather than hostility or indifference.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What is the author's overall tone toward the company's initiatives?",
                    MainText         = "While acknowledging the company's efforts to innovate, the article subtly hints that many initiatives may be more about optics than meaningful change. Phrases such as 'well-publicised' and 'frequently highlighted in press releases' suggest that the projects are designed to be noticed rather than transformative. The author's commentary balances recognition of ambition with mild scepticism, leaving the reader to question the depth of the initiatives.",
                    Option1          = "Completely supportive",
                    Option2          = "Cautiously sceptical",
                    Option3          = "Neutral",
                    Option4          = "Disbelieving with outrage",
                    Option5          = "Confused",
                    CorrectAnswer    = "Cautiously sceptical",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The initiatives are acknowledged, but the scepticism is mild and measured. The phrasing 'subtly hints' and 'mild scepticism' rule out full support or outright hostility.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What is the main causal chain the passage describes?",
                    MainText         = "Traffic congestion worsened after the city expanded several major roads without improving public transit options. Commuters increasingly chose private cars over buses, creating a feedback loop that led to longer travel times. City planners noted that convenience, habit, and availability all contributed to the problem, suggesting that physical infrastructure alone could not solve congestion. The passage highlights the interplay between infrastructure decisions and human behaviour.",
                    Option1          = "Traffic increased because people dislike buses",
                    Option2          = "Road expansion alone caused congestion",
                    Option3          = "Road expansion plus limited transit led to more cars, increasing congestion",
                    Option4          = "Longer travel times caused more cars to appear",
                    Option5          = "Commuters prefer private cars for comfort",
                    CorrectAnswer    = "Road expansion plus limited transit led to more cars, increasing congestion",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "The passage explicitly links road expansion and limited transit to increased car usage and then to congestion. The other options omit key parts of this causal chain.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What does the passage conclude about recycling incentives?",
                    MainText         = "After introducing a recycling incentive program, the city noted a modest increase in participation. Surveys suggested the incentive motivated only a portion of residents; habits and convenience remained major factors influencing behaviour. Officials concluded that while incentives help, structural and behavioural considerations play a critical role in adoption. The passage emphasises that change is multifaceted and not solely determined by financial incentives.",
                    Option1          = "Incentives alone are sufficient to change behaviour",
                    Option2          = "Residents ignored the program entirely",
                    Option3          = "The survey was unreliable",
                    Option4          = "Habit and convenience limit the effectiveness of incentives",
                    Option5          = "Recycling rates decreased",
                    CorrectAnswer    = "Habit and convenience limit the effectiveness of incentives",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Incentives had some effect, but habit and convenience remained important limiting factors — the central message of the passage.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "How do the two diet studies compare?",
                    MainText         = "Two recent studies on diet and sleep show similar overall patterns: high sugar intake correlates with shorter sleep duration. Study A, which had a smaller sample, found stronger correlations than Study B, which included a larger and more diverse population. Both studies noted limitations and called for further research to explore the relationship under different conditions. The findings highlight a trend but also suggest caution in interpreting the magnitude of effects.",
                    Option1          = "Study A is more reliable than Study B",
                    Option2          = "Study B is unreliable",
                    Option3          = "Sugar has no impact on sleep",
                    Option4          = "The studies contradict each other entirely",
                    Option5          = "Both studies indicate a sugar–sleep relationship, but effect sizes vary",
                    CorrectAnswer    = "Both studies indicate a sugar–sleep relationship, but effect sizes vary",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "Both studies align in direction; differences in effect size are due to methodology and sample diversity, not contradiction.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What does the comparison of the two cities suggest?",
                    MainText         = "One city implemented strict water restrictions, while another emphasised public education campaigns. Both cities saw modest reductions in water usage, though the restriction-based city showed slightly faster short-term impact. Officials noted that the long-term sustainability of reductions depended on a mix of behavioural and regulatory factors. The passage suggests that different approaches can achieve similar ends, with nuances in speed and persistence.",
                    Option1          = "Restrictions are always better than education",
                    Option2          = "Education has no effect",
                    Option3          = "Water conservation is impossible",
                    Option4          = "Different approaches can achieve similar results, with nuances in timing",
                    Option5          = "Public compliance is irrelevant",
                    CorrectAnswer    = "Different approaches can achieve similar results, with nuances in timing",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Both methods produced reductions; the difference was in timing and persistence. Overgeneralising either method as superior would be misleading.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What does the word 'idiosyncratic' most likely mean in this passage?",
                    MainText         = "The artist's latest work was described as 'idiosyncratic,' blending familiar motifs with unexpected materials to produce a style that feels personal and occasionally eccentric. Critics noted that while some pieces were immediately engaging, others challenged traditional expectations and may not appeal to all audiences. Overall, the exhibition was praised for originality, though viewers were divided on particular interpretations.",
                    Option1          = "Unique to the artist",
                    Option2          = "Standard and conventional",
                    Option3          = "Confusing or chaotic",
                    Option4          = "Low quality",
                    Option5          = "Typical of many artists",
                    CorrectAnswer    = "Unique to the artist",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Context clues — 'personal,' 'occasionally eccentric,' and 'praised for originality' — all point toward a style that is distinctively the artist's own.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "How would you best evaluate the editorial's argument about arts funding?",
                    MainText         = "An editorial argued that government spending on arts programs is wasteful, claiming funds could be better spent on infrastructure. However, the argument selectively focused on costs and did not consider potential economic or cultural benefits of arts funding. Critics of the editorial suggested that the piece reflects an opinion supported by incomplete evidence rather than a definitive fact. Readers are left to weigh the reasoning against their own assessment of priorities.",
                    Option1          = "The claim is purely factual",
                    Option2          = "The data conclusively proves wastefulness",
                    Option3          = "The claim is an opinion, supported by selective evidence",
                    Option4          = "Cultural benefits are irrelevant",
                    Option5          = "Government should not fund infrastructure",
                    CorrectAnswer    = "The claim is an opinion, supported by selective evidence",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "The argument reflects an opinion built on incomplete evidence. The other options either overstate the certainty of the claim or introduce unrelated ideas.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "What does the word 'tentative' most likely mean in this passage?",
                    MainText         = "The report labelled several initiatives as 'tentative,' acknowledging that the results were promising but still preliminary and requiring further validation. Analysts emphasised that early successes do not guarantee long-term effectiveness, and that each initiative would need continuous assessment. The term 'tentative' captures the cautious optimism present throughout the report.",
                    Option1          = "Definitive and final",
                    Option2          = "Risky or dangerous",
                    Option3          = "Unimportant",
                    Option4          = "Hesitant or provisional",
                    Option5          = "Well-established",
                    CorrectAnswer    = "Hesitant or provisional",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "The context — 'promising but still preliminary' and 'requiring further validation' — makes clear that 'tentative' means provisional rather than final or risky.",
                    Category         = QuestionCategory.ReadingComprehension
                },
                new Question
                {
                    QuestionText     = "How would you best describe the journalist's tone?",
                    MainText         = "While praising the city's ambitious urban renewal plan, the journalist noted that 'ambition alone cannot guarantee success,' hinting that challenges such as funding, community buy-in, and unforeseen complications could limit results. The coverage acknowledged both the promise and the potential pitfalls, suggesting that careful implementation would be essential. Overall, the piece balanced admiration for initiative with caution about practical hurdles.",
                    Option1          = "Fully supportive",
                    Option2          = "Entirely sceptical",
                    Option3          = "Neutral and detached",
                    Option4          = "Hostile",
                    Option5          = "Optimistic but cautiously realistic",
                    CorrectAnswer    = "Optimistic but cautiously realistic",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "The journalist acknowledges merits and emphasises realistic limitations. Extreme optimism or cynicism is absent, making this the best fit.",
                    Category         = QuestionCategory.ReadingComprehension
                }
            );

            if (!hasShortTermMemory)
            context.Questions.AddRange(
                new Question
                {
                    QuestionText     = "Which department handles billing questions?",
                    MainText         = "During a team orientation, the manager explained how different types of requests are handled across the company. Customer complaints should always be forwarded to the support team, billing questions must go to the finance department for proper review, and any technical issues are addressed by the engineering team. Urgent requests need to be clearly labeled so they can be prioritised. Weekly summaries of all requests are shared with management every Friday to keep everyone updated on recurring issues and performance trends.",
                    Option1          = "The finance department, which reviews billing questions and payment-related concerns",
                    Option2          = "The support team, which deals with customer complaints and general enquiries",
                    Option3          = "The engineering team, which handles technical issues and system errors",
                    Option4          = "The management team, which receives weekly summaries but does not handle questions directly",
                    Option5          = "The operations team, which oversees day-to-day logistics but is not mentioned in this context",
                    CorrectAnswer    = "The finance department, which reviews billing questions and payment-related concerns",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "Billing questions are explicitly assigned to the finance department. The other options describe roles mentioned in the passage but are not responsible for billing — keeping all details in mind is key for accuracy.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "Which of the following was mentioned as a festival attraction?",
                    MainText         = "A radio host described a weekend community festival with several attractions. She said the event would feature live music performances, a variety of food trucks, a small craft market with handmade items, and fun activities for children. The festival takes place in a public park, and admission is free for all attendees. The host emphasised that the combination of these features makes it a fun event for families and encourages everyone to come early to enjoy all the activities without rushing.",
                    Option1          = "Fireworks displayed in the evening sky as the main entertainment",
                    Option2          = "A series of live music performances featuring local bands and singers",
                    Option3          = "A parade with floats and marching bands throughout the park",
                    Option4          = "Sports competitions including races and team games for visitors",
                    Option5          = "A charity auction to raise money for local organisations during the festival",
                    CorrectAnswer    = "A series of live music performances featuring local bands and singers",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The passage specifically mentions live music as an attraction. The other options sound plausible for a festival but were not included, so holding the correct details in mind is essential.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "How many academic activities did the student plan?",
                    MainText         = "A student reviewed her schedule before leaving for the day. She planned to attend a morning lecture on psychology, spend an hour studying in the library, meet a friend for lunch at the campus café, and go to an afternoon lab session for her biology course. She also planned to relax in the evening with some reading, but she did not consider that an academic task. Keeping track of all of these commitments was important so she could manage her time efficiently throughout the day.",
                    Option1          = "Only one activity, the morning lecture on psychology",
                    Option2          = "Two activities, the lecture and the lunch meeting with a friend",
                    Option3          = "Four activities, including the lecture, study session, lunch meeting, and lab session",
                    Option4          = "Three activities, the lecture, the study session, and the lab session",
                    Option5          = "Five activities, including all planned events including evening relaxation",
                    CorrectAnswer    = "Four activities, including the lecture, study session, lunch meeting, and lab session",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "The student had four academic activities: the lecture, study session, lunch meeting, and lab session. Evening relaxation was not considered academic, so careful attention is needed to select the correct number.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "Which habit was NOT mentioned as a way to improve focus?",
                    MainText         = "An article described simple habits for improving focus at work. The author recommended taking short breaks between tasks, reducing unnecessary notifications on devices, organising tasks before beginning work, and staying hydrated throughout the day. She emphasised that these small, consistent changes can make a noticeable difference in concentration without requiring major lifestyle adjustments. By maintaining a few focused practices, employees can improve attention and reduce the likelihood of distraction in a busy office environment.",
                    Option1          = "Taking short breaks to rest and reset attention throughout the workday",
                    Option2          = "Reducing notifications on phones and computers to limit interruptions",
                    Option3          = "Drinking enough water to stay hydrated and alert",
                    Option4          = "Exercising at the gym to increase energy and mental clarity",
                    Option5          = "Organising tasks ahead of time to streamline workflow and reduce stress",
                    CorrectAnswer    = "Exercising at the gym to increase energy and mental clarity",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Exercise is not mentioned in the passage. The other habits are all described as ways to maintain focus, so the key is remembering what was included versus what was not.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "What is the current status of testing?",
                    MainText         = "During a project update, a manager explained the current status of several tasks. She said the planning phase was complete, development was currently in progress, and testing had not yet begun. She also noted that the project deadline remained unchanged and promised that another update would be shared next week. Keeping track of all these details helped the team understand which parts of the project were active, which had finished, and which tasks were upcoming.",
                    Option1          = "Testing is complete and all results have been documented",
                    Option2          = "Testing is underway and progressing alongside development tasks",
                    Option3          = "Testing has been cancelled for this phase of the project",
                    Option4          = "Testing is behind schedule but some parts have been completed",
                    Option5          = "Testing has not yet started and will begin later",
                    CorrectAnswer    = "Testing has not yet started and will begin later",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "The passage clearly states that testing has not yet begun. Other options introduce changes not mentioned, making it important to hold the original wording in memory.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "Which city had the higher temperature?",
                    MainText         = "A weather report compared conditions in two neighbouring cities for the day. City A reached a high of 74 degrees with light winds and sunny skies. City B was slightly cooler with a high of 69 degrees, but also enjoyed clear conditions and gentle breezes. The reporter mentioned that both cities were expected to have pleasant afternoons, encouraging residents to enjoy outdoor activities.",
                    Option1          = "City A, which reached 74 degrees with clear skies and light winds",
                    Option2          = "City B, which reached 69 degrees and also had pleasant weather conditions",
                    Option3          = "Both cities had the same temperature according to the report",
                    Option4          = "Neither city had temperatures mentioned in the report",
                    Option5          = "The report did not provide temperature information for either city",
                    CorrectAnswer    = "City A, which reached 74 degrees with clear skies and light winds",
                    BestAnswers      = new[] { 1 },
                    ExplanationText  = "City A's temperature was higher at 74 compared with 69 for City B. Retaining both numbers and comparing them correctly is the main memory challenge here.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "Which item did she forget to buy?",
                    MainText         = "A character described a recent shopping trip, recalling that she bought bread, milk, fruit, and coffee. However, she later realised she had forgotten to buy eggs, which she had intended to purchase. When she thought back on the trip, only the items she actually purchased came to mind, while the forgotten item stood out because it had been missed.",
                    Option1          = "Bread, which she successfully purchased at the store",
                    Option2          = "Milk, which she picked up without issue",
                    Option3          = "Fruit, which was remembered and purchased",
                    Option4          = "Coffee, which was bought and brought home",
                    Option5          = "Eggs, which she intended to buy but forgot",
                    CorrectAnswer    = "Eggs, which she intended to buy but forgot",
                    BestAnswers      = new[] { 5 },
                    ExplanationText  = "The passage highlights that eggs were the item she forgot. This is a classic selective recall task: remembering the exception rather than the items that were successfully purchased.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "How will feedback be collected?",
                    MainText         = "During a short training session, an instructor explained how feedback would be collected. Participants would complete a short, anonymous survey at the end of the session. The instructor emphasised that honest and detailed feedback was important so the training could be improved. Responses would be reviewed the following week to assess participant satisfaction and identify areas for adjustment.",
                    Option1          = "Through in-person interviews conducted individually after the session",
                    Option2          = "By completing a short, anonymous survey distributed to all participants",
                    Option3          = "During group discussion sessions where everyone shares opinions aloud",
                    Option4          = "Via email responses submitted after the session concludes",
                    Option5          = "In written reports submitted manually to the instructor's office",
                    CorrectAnswer    = "By completing a short, anonymous survey distributed to all participants",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The instructor specifies a short, anonymous survey. Other options describe methods not mentioned, so holding the passage in mind is key.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "Were any layoffs planned?",
                    MainText         = "A company representative announced upcoming changes at a brief meeting. Office hours would remain the same, remote work options would continue, and a new internal tool would be introduced next month. She reassured staff that no layoffs were planned and encouraged employees to adopt the new tool to improve workflow efficiency. This announcement was meant to clarify what would change and what would remain the same.",
                    Option1          = "Yes, immediate layoffs were scheduled to take effect",
                    Option2          = "Yes, layoffs were planned later in the year for some employees",
                    Option3          = "No, layoffs were not planned and staff could remain confident",
                    Option4          = "The announcement did not clarify whether layoffs were occurring",
                    Option5          = "Only specific departments would face layoffs while others remained safe",
                    CorrectAnswer    = "No, layoffs were not planned and staff could remain confident",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "The passage explicitly states that no layoffs were planned. Retaining this detail requires attention to both the changes and the reassurance sections of the announcement.",
                    Category         = QuestionCategory.ShortTermMemory
                },
                new Question
                {
                    QuestionText     = "Which items required senior leadership approval?",
                    MainText         = "During a brief meeting, a supervisor outlined which items required approval. She said that the budget proposal and the hiring plan needed approval from senior leadership, while the updated schedule could be finalised by the team without additional review. She emphasised these distinctions to ensure clarity and avoid mistakes, so the team understood which items required higher-level authorisation and which could be handled independently.",
                    Option1          = "Only the updated schedule, which could be finalised by the team",
                    Option2          = "The budget proposal only, leaving the hiring plan for team approval",
                    Option3          = "The hiring plan only, with the budget proposal managed by the team",
                    Option4          = "Both the budget proposal and the hiring plan required senior leadership approval",
                    Option5          = "All three items, including the updated schedule, needed approval",
                    CorrectAnswer    = "Both the budget proposal and the hiring plan required senior leadership approval",
                    BestAnswers      = new[] { 4 },
                    ExplanationText  = "Both the budget proposal and the hiring plan required approval. The schedule did not. Remembering these distinctions is an important aspect of short-term memory for multi-part information.",
                    Category         = QuestionCategory.ShortTermMemory
                }
            );

            if (!hasConfidenceCalibration)
            context.Questions.AddRange(
                new Question
                {
                    QuestionText     = "How confident should you be in the scientist's prediction?",
                    MainText         = "A junior scientist working in pharmaceutical research publicly states that she is 95% confident that a newly developed drug will turn out to be a major medical breakthrough. She has published several peer-reviewed papers and appears technically capable, but she has never led a full clinical trial. The drug targets a disease that is still poorly understood, with few successful treatments so far, and the project is at an early stage.",
                    Option1          = "Moderately confident, 50–65%",
                    Option2          = "Very confident, 80–90%",
                    Option3          = "Fully confident, 95% like her",
                    Option4          = "Slightly confident, 30–40%",
                    Option5          = "Not confident at all",
                    CorrectAnswer    = "Moderately confident, 50–65%",
                    BestAnswers      = new[] { 1, 2 },
                    ExplanationText  = "While the scientist has some expertise, the novelty of the drug and her limited experience suggest substantial uncertainty. Moderate confidence is reasonable. Slightly lower confidence is also rational, particularly given the general unpredictability of early-stage drug development. Her assertion appears to be poorly calibrated, which should reduce your trust in the prediction.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "Which betting option best reflects a well-calibrated choice?",
                    MainText         = "You are asked to place a points-based bet on the approximate number of countries in the world. You may choose a narrow range for a higher reward but greater risk, a wider range for safer points, or decline to bet entirely if you feel unsure.",
                    Option1          = "180–185, gain 80 points if correct, lose 30 if wrong",
                    Option2          = "190–200, gain 75 points if correct, lose 20 if wrong",
                    Option3          = "190–195, gain 100 points if correct, lose 40 if wrong",
                    Option4          = "185–190, gain 90 points if correct, lose 35 if wrong",
                    Option5          = "Decline to bet",
                    CorrectAnswer    = "190–200, gain 75 points if correct, lose 20 if wrong",
                    BestAnswers      = new[] { 2 },
                    ExplanationText  = "The true number is 195 recognised countries. Narrow intervals like A or D are incorrect. C is optimal but risky unless you are very sure. B is slightly wider, providing a safer balance between reward and risk. Declining to bet is reasonable if your knowledge is limited — it reflects good calibration and avoids unnecessary loss.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "What is the minimum number of these invention statements you believe are correct?",
                    MainText         = "Below are six statements about well-known inventions and discoveries. Some are commonly taught facts, while others may rely on simplified historical narratives. You do not need to determine which specific statements are true. Instead, indicate the minimum number you believe must be correct.\n1. The lightbulb was invented by Thomas Edison.\n2. The telephone was invented by Alexander Graham Bell.\n3. The first automobile was powered by steam.\n4. The printing press was invented before the 16th century.\n5. Penicillin was discovered by Alexander Fleming.\n6. The Wright brothers flew the first powered airplane.",
                    Option1          = "At least 1",
                    Option2          = "At least 2",
                    Option3          = "At least 4",
                    Option4          = "At least 3",
                    Option5          = "All 6",
                    CorrectAnswer    = "At least 4",
                    BestAnswers      = new[] { 3 },
                    ExplanationText  = "Statements 1, 2, 4, 5, and 6 are true; statement 3 is false. Choosing at least 4 shows awareness that most statements are correct but avoids assuming all six are true. Lower minimums show underconfidence, while higher minimums reflect overconfidence. If unsure, a slightly lower minimum is also reasonable.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "Which interval best reflects your calibrated confidence about the iPhone release year?",
                    MainText         = "Suppose you are trying to recall the year the first iPhone was released. You have a general sense of the time period but may not remember the exact year. Narrower windows offer higher payoffs but are riskier, while wider intervals are safer. You may also decline to bet if you are too uncertain.",
                    Option1          = "2004–2006, gain 80 points, lose 30",
                    Option2          = "2002–2005, gain 90 points, lose 35",
                    Option3          = "2006–2009, gain 75 points, lose 20",
                    Option4          = "2007–2008, gain 100 points, lose 40",
                    Option5          = "Decline to bet",
                    CorrectAnswer    = "2007–2008, gain 100 points, lose 40",
                    BestAnswers      = new[] { 4, 3, 5 },
                    ExplanationText  = "The iPhone was launched in 2007. Options A and B are losses. D is the optimal choice for someone confident in the answer; C is slightly wider and reduces risk if slightly uncertain. Declining is rational if your knowledge is limited. The key lesson is that betting smaller, safer intervals or declining can be smart when uncertainty is high.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "How should the engineer's 99% confidence claim be evaluated?",
                    MainText         = "A highly experienced structural engineer reviews the final design plans for a proposed suspension bridge. After careful analysis, running multiple simulations, and checking the plans against established safety standards, she concludes that the bridge would almost certainly be unsafe if built exactly as designed. She reports being 99% confident in her judgment. The engineer has designed dozens of bridges, follows rigorous regulatory processes, and works in a field governed by well-understood physical laws.",
                    Option1          = "The claim is almost certainly incorrect; 99% is too extreme",
                    Option2          = "The claim is somewhat exaggerated; maybe 70–80% confidence is warranted",
                    Option3          = "The claim is uncertain; only 50–60% confidence is warranted",
                    Option4          = "The claim is fully credible; 99% confidence is appropriate",
                    Option5          = "The claim is highly credible; about 90% confidence is appropriate",
                    CorrectAnswer    = "The claim is highly credible; about 90% confidence is appropriate",
                    BestAnswers      = new[] { 5, 4 },
                    ExplanationText  = "Civil and structural engineering is a highly structured, well-studied domain. Experts use validated principles, extensive testing, and established safety margins. In such environments, very high confidence is reasonable when evidence points clearly to a risk. While rare unknown factors might justify slightly lower confidence, the rigor of engineering practice makes very high confidence fully defensible.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "What is the minimum number of these geography statements you believe are correct?",
                    MainText         = "Below are five statements about geography, including population facts, size comparisons, and well-known natural features. Some statements may sound surprising but are accurate, while others reflect common misconceptions. Indicate the minimum number you believe are true.\n1. Iceland has more sheep than people.\n2. Australia is wider east–west than Greenland.\n3. The Sahara is the largest desert by area.\n4. Japan has over 6,000 islands.\n5. The Nile is longer than the Amazon.",
                    Option1          = "At least 1",
                    Option2          = "At least 2",
                    Option3          = "At least 3",
                    Option4          = "At least 4",
                    Option5          = "All 5",
                    CorrectAnswer    = "At least 3",
                    BestAnswers      = new[] { 2, 3, 4 },
                    ExplanationText  = "Statements 1, 2, 3, and 4 are true; statement 5 is false. Choosing at least 3 or 4 shows reasonable calibration. Lower minimums are overly cautious; higher minimums suggest overconfidence. If you are unsure about specific statements, selecting a smaller minimum is sensible to avoid being overconfident.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "Which interval best reflects a well-calibrated estimate for when the first email was sent?",
                    MainText         = "You are asked to estimate the approximate year when the first email was sent. You know it occurred early in the history of networked computers, but the exact date may be unclear. Narrower intervals offer higher rewards but higher risk; wider intervals are safer.",
                    Option1          = "1969–1972, gain 100 points, lose 35",
                    Option2          = "1968–1969, gain 90 points, lose 30",
                    Option3          = "1965–1968, gain 80 points, lose 25",
                    Option4          = "1970–1975, gain 75 points, lose 20",
                    Option5          = "Decline to bet",
                    CorrectAnswer    = "1969–1972, gain 100 points, lose 35",
                    BestAnswers      = new[] { 1, 4 },
                    ExplanationText  = "The first email was sent in 1971. Narrow intervals are tempting but risky if uncertain. D offers a slightly safer balance. A contains the correct year and offers the highest reward if you know the answer. Declining to bet is reasonable if your knowledge is limited — recognising your own limits is a core part of calibration.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "How confident should you be in the hedge fund manager's claim?",
                    MainText         = "A hedge fund manager states with extreme confidence that a new investment strategy will outperform the overall market this year. She has an impressive history of strong returns and clear expertise, but current market conditions are volatile, and many external factors are unpredictable. Past performance does not guarantee future results.",
                    Option1          = "Fully confident, 85%",
                    Option2          = "Very confident, 65–75%",
                    Option3          = "Moderately confident, 50–60%",
                    Option4          = "Slightly confident, 25–35%",
                    Option5          = "Not confident at all",
                    CorrectAnswer    = "Very confident, 65–75%",
                    BestAnswers      = new[] { 2, 3 },
                    ExplanationText  = "Even experienced professionals can be wrong in volatile markets. Moderate to moderately-high confidence is appropriate. Choosing slightly lower confidence is also rational, reflecting awareness of uncertainty. The manager's overconfidence may indicate poor calibration and should reduce your trust in the prediction.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "Which interval best reflects a well-calibrated estimate for the length of the Great Wall?",
                    MainText         = "You are asked to estimate the total length of the Great Wall of China. You know it spans thousands of kilometres and includes many disconnected sections built over centuries, but the exact number is difficult to recall. Choose the interval that best matches your confidence level, or decline to bet.",
                    Option1          = "10,000–12,000 km, gain 80 points, lose 30",
                    Option2          = "12,000–13,000 km, gain 90 points, lose 35",
                    Option3          = "13,000–14,000 km, gain 100 points, lose 40",
                    Option4          = "12,500–14,500 km, gain 75 points, lose 20",
                    Option5          = "Decline to bet",
                    CorrectAnswer    = "12,500–14,500 km, gain 75 points, lose 20",
                    BestAnswers      = new[] { 3, 4 },
                    ExplanationText  = "The Great Wall is approximately 13,171 km. Options A and B are losses. C is optimal if you know the answer precisely; D is a good choice if you have some knowledge but want to reduce risk by using a slightly wider interval. Declining is appropriate if you realise your knowledge is limited. Being cautious is a valid strategy when facts are uncertain.",
                    Category         = QuestionCategory.ConfidenceCalibration
                },
                new Question
                {
                    QuestionText     = "What is the minimum number of these biology statements you believe are correct?",
                    MainText         = "Below are six statements about biology, covering genetics, animal classification, and survival mechanisms. Some are basic facts, while others may challenge intuition. Indicate the minimum number you believe are true.\n1. Humans have 46 chromosomes.\n2. Sharks are mammals.\n3. Some birds cannot fly.\n4. Some metals are liquid at room temperature.\n5. Some reptiles regulate their body temperature internally.\n6. Some insects can survive freezing temperatures.",
                    Option1          = "At least 1",
                    Option2          = "At least 2",
                    Option3          = "At least 5",
                    Option4          = "At least 4",
                    Option5          = "At least 3",
                    CorrectAnswer    = "At least 3",
                    BestAnswers      = new[] { 5, 4 },
                    ExplanationText  = "Statements 1, 3, 4, and 6 are true; statements 2 and 5 are false. Choosing at least 3 or 4 shows awareness that several statements are correct while leaving room for error. If unsure, selecting a lower minimum is reasonable — cautious judgments are valuable when facts are partially unknown.",
                    Category         = QuestionCategory.ConfidenceCalibration
                }
            );
            await context.SaveChangesAsync();
        }
    }
}



