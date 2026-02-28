-- Run this script against aireliefdb.db to restore the original questions.
-- Safe to re-run: each INSERT is guarded by a NOT EXISTS check on the maintext/questiontext.

-- ============================================================
-- Trial questions
-- ============================================================

INSERT INTO Questions (maintext, image, explanationtext, explanationimage, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, Category)
SELECT
    'A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?',
    'q1.png',
    'At first glance, you might think the eraser costs $0.50 since the pencil costs 50 cents more. But, if the pencil costs $0.75 and the eraser costs $0.25, together they add up to $1.00. The first instinct is often to assume the eraser costs more because of how the question is framed, but the correct breakdown is $0.25 for the eraser and $0.75 for the pencil.',
    'q1x.png',
    '$0.50', '$0.25', '$0.75', '$0.05', NULL,
    '$0.25', 'Trial'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE maintext = 'A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?');

INSERT INTO Questions (maintext, image, explanationtext, explanationimage, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, Category)
SELECT
    'In a race, you pass the person in second place. What place are you in now?',
    'q2.png',
    'It''s easy to think you''re now in 1st place, but if you pass the person in second place, you''re now in 2nd, not 1st. The person in 1st is still ahead of you.',
    'q2x.png',
    '2nd', '1st', '3rd', '4th', NULL,
    '2nd', 'Trial'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE maintext = 'In a race, you pass the person in second place. What place are you in now?');

INSERT INTO Questions (maintext, explanationtext, explanationimage, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, Category)
SELECT
    'If a train leaves New York at 10:00 AM and travels at 60 miles per hour, and another train leaves the same station at the same time but travels at 90 miles per hour, how long will it take before the second train catches up to the first?',
    'This is a trick question. The second train is faster, but both trains are leaving from the same station at the same time. They''ll never catch up because they''re already on the same path, just traveling at different speeds.',
    'q3x.png',
    '1 hour', '30 minutes', 'They will never meet', '2 hours', NULL,
    'They will never meet', 'Trial'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE maintext = 'If a train leaves New York at 10:00 AM and travels at 60 miles per hour, and another train leaves the same station at the same time but travels at 90 miles per hour, how long will it take before the second train catches up to the first?');

INSERT INTO Questions (maintext, image, explanationtext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, Category)
SELECT
    'A car travels 30 miles in 30 minutes. What is the average speed of the car?',
    'q4.png',
    'The car travels 30 miles in 30 minutes, which is the same as 0.5 hours. So, the average speed is 30 miles / 0.5 hours = 60 miles per hour. The trick is in interpreting the time properly and not letting the "30 minutes" confuse you.',
    '60 miles per hour', '30 miles per hour', '15 miles per hour', '2 miles per minute', NULL,
    '60 miles per hour', 'Trial'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE maintext = 'A car travels 30 miles in 30 minutes. What is the average speed of the car?');

-- ============================================================
-- Causal Reasoning questions
-- ============================================================

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why the bike-sharing program may not be the real cause of fewer accidents?',
    'A small town introduces a bike-sharing program, and over the next year, local traffic accidents decline. Officials suggest that more people cycling reduces car use, improving safety. However, the program coincides with the installation of new traffic lights, a public awareness campaign on road safety, and the repaving of several major roads. Some residents also reported that traffic patterns shifted due to new commuting options. This combination of changes makes it difficult to know which factor truly accounts for the drop in accidents.',
    'Traffic lights, campaigns, and repaved roads could explain the decline',
    'Cyclists may still get into accidents',
    'People might dislike cycling in bad weather',
    'Bicycle ownership is unrelated to traffic accidents',
    'The town''s population may have decreased',
    'Traffic lights, campaigns, and repaved roads could explain the decline', '1',
    'Multiple interventions occurred simultaneously, each of which could plausibly influence accident rates. While cycling might contribute, attributing the change solely to the bike-sharing program ignores these other factors.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why the bike-sharing program may not be the real cause of fewer accidents?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why the study groups may not be the real cause of better grades?',
    'Researchers observe that students who participate in study groups earn higher grades. They conclude that study groups improve academic performance. Participation is voluntary, and more diligent or highly motivated students are often overrepresented in these groups. Some students also reported that they seek study groups only when preparing for particularly challenging courses. Without data tracking students'' prior habits or comparing them to similar students who do not join groups, it is hard to know if study groups are the cause or simply correlated with motivated students.',
    'Students enjoy socializing',
    'More motivated or diligent students may choose to join',
    'Grades are subjective',
    'Some study groups are larger than others',
    'Professors adjust difficulty each semester',
    'More motivated or diligent students may choose to join', '2',
    'Self-selection into study groups could account for the higher grades. Motivation and prior diligence may explain why students both join groups and perform well, making it unclear whether the study group itself caused the improvement.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why the study groups may not be the real cause of better grades?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best describes why we cannot be sure flexible hours caused the rise in satisfaction?',
    'A company introduces flexible work hours, and employee reports of job satisfaction increase over six months. Management credits the policy for this improvement. At the same time, the company upgraded office amenities, improved team communication practices, and encouraged employees to attend professional development workshops. Employees'' job responsibilities also shifted slightly, giving more autonomy to certain teams. With so many concurrent changes, it is difficult to isolate the effect of flexible hours alone.',
    'Flexible hours are the sole cause of higher satisfaction',
    'Upgraded amenities alone caused higher satisfaction',
    'Multiple simultaneous changes make it unclear which factor mattered most',
    'Satisfaction always increases after policy changes',
    'Employee reports are irrelevant',
    'Multiple simultaneous changes make it unclear which factor mattered most', '3',
    'Several changes occurred at once, any of which could influence satisfaction. A careful interpretation acknowledges that flexible hours may contribute, but the data do not show that they are the only cause.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best describes why we cannot be sure flexible hours caused the rise in satisfaction?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why the gardens may not be the real cause of lower crime?',
    'A city observes that neighbourhoods with more community gardens have lower crime rates. City planners suggest that gardens reduce criminal activity by fostering social cohesion and improving neighbourhood engagement. However, these same neighbourhoods tend to have more active community organisations, higher average income, and lower rates of vacant properties. Without accounting for these other characteristics, it is difficult to know whether the gardens themselves reduce crime or if they are simply more common in safer, more organised neighbourhoods.',
    'Gardens take up space',
    'Crime varies randomly',
    'Plants require maintenance',
    'Higher income and active associations may explain lower crime',
    'Residents might report crime differently',
    'Higher income and active associations may explain lower crime', '4',
    'Other neighbourhood features, such as income and social organisation, could be driving the lower crime rates. Gardens may contribute, but the observed association may largely reflect these other factors.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why the gardens may not be the real cause of lower crime?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why drinking tea may not be the real cause of fewer colds?',
    'A health magazine reports that people who drink herbal tea daily report fewer colds. No longitudinal data are available, and survey respondents are self-selected, often including individuals who already follow other healthy behaviours such as exercising, taking vitamins, or getting sufficient sleep. Without tracking these factors, it is unclear whether tea itself has any protective effect, or whether people who drink tea are simply more likely to engage in other behaviours that reduce illness. The magazine promotes the tea as a direct remedy for colds.',
    'Herbal tea tastes good',
    'Survey participation is voluntary',
    'Colds are unpredictable',
    'Tea is expensive',
    'Health-conscious individuals may both drink tea and take other preventive measures',
    'Health-conscious individuals may both drink tea and take other preventive measures', '5',
    'People who already practise healthy habits may also drink tea, creating a confounding factor. The association between tea consumption and fewer colds does not necessarily reflect a causal effect.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why drinking tea may not be the real cause of fewer colds?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why the mentorship programme may not be the real cause of greater confidence?',
    'A small company introduces a mentorship programme. Six months later, employees who participate report higher confidence in handling tasks. Management attributes this change to the mentorship programme. However, the employees selected for mentorship were among the most ambitious and already had strong performance records. In addition, some participants had recently completed external training courses. Without accounting for these pre-existing differences, it is difficult to conclude that the mentorship programme alone caused the improvement in confidence.',
    'Mentors are paid',
    'Selection bias based on ambition and past performance',
    'Confidence cannot be measured',
    'Only a few employees participated',
    'Tasks became easier',
    'Selection bias based on ambition and past performance', '2',
    'Employees chosen for mentorship may have improved confidence regardless of the programme. Selection bias complicates the causal interpretation.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why the mentorship programme may not be the real cause of greater confidence?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why the longer hours may not be the real cause of higher library attendance?',
    'After a local library extends evening hours, attendance rises noticeably over several months. Library officials conclude that longer hours directly caused the increase. However, the library also ran a citywide literacy campaign, introduced new reading programmes for teens, and improved the online catalogue at the same time. Some residents mentioned that they were motivated by both the programmes and the extended hours. The combination of multiple interventions makes it unclear which factor drove the increase in attendance.',
    'Promotional campaigns and new programmes may have increased attendance',
    'Evening hours might be inconvenient',
    'Libraries cannot influence reading habits',
    'Attendance is always seasonal',
    'Some residents do not visit libraries',
    'Promotional campaigns and new programmes may have increased attendance', '1',
    'Multiple simultaneous changes could explain the rise in attendance. The extended hours may contribute, but they cannot be identified as the sole cause.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why the longer hours may not be the real cause of higher library attendance?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why walking may not be the real cause of lower stress?',
    'A study finds that employees who take short afternoon walks report lower stress levels than those who do not. Researchers suggest walking reduces stress. However, employees who take walks tend to have flexible schedules, supportive managers, and less demanding workloads. Some also engage in mindfulness exercises during breaks. These other factors could plausibly explain why stress levels are lower, making it difficult to isolate the effect of walking alone.',
    'Walking has no physical benefits',
    'Employees may exaggerate their stress',
    'Stress is subjective',
    'Other workplace factors, like flexible schedules or support, could account for lower stress',
    'Walks only occur on sunny days',
    'Other workplace factors, like flexible schedules or support, could account for lower stress', '4',
    'Lower stress may be due to multiple aspects of the work environment, not just walking. The evidence does not clearly isolate walking as the causal factor.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why walking may not be the real cause of lower stress?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best describes what we can reasonably conclude about the rise in productivity?',
    'A tech company implements a new task management system. During the same quarter, productivity metrics improve noticeably. However, several teams also received training on time management, and managers began using new performance tracking tools. Some employees reported that these concurrent changes made it easier to complete tasks efficiently. Because multiple interventions occurred simultaneously, it is difficult to determine whether the task management system itself caused the improvement.',
    'The new system caused all productivity gains',
    'Training alone explains productivity',
    'Both the system and training could have contributed',
    'Productivity is unrelated to the system',
    'Metrics are meaningless',
    'Both the system and training could have contributed', '3',
    'Multiple changes occurred at once, any of which could influence productivity. A careful interpretation acknowledges that the system may contribute, but it is likely not the only factor.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best describes what we can reasonably conclude about the rise in productivity?');

INSERT INTO Questions (questiontext, maintext, Option1, Option2, Option3, Option4, Option5, CorrectAnswer, BestAnswersRaw, explanationtext, Category)
SELECT
    'Which answer best explains why the festival may not be the real cause of higher hotel bookings?',
    'A city observes that during months when it hosts a music festival, local hotel bookings increase substantially. Officials suggest the festival drives tourism. However, these months also coincide with school holidays, warmer weather, and a regional food festival. Some residents report that visitors come for multiple reasons, not just the music festival. Without controlling for these other seasonal factors, it is difficult to know how much of the increase in bookings is truly caused by the festival itself.',
    'Hotels are more attractive in summer',
    'Local restaurants may also attract visitors',
    'Festivals are loud',
    'Bookings depend on online platforms',
    'Tourism may rise due to holidays, weather, or other events, independent of the festival',
    'Tourism may rise due to holidays, weather, or other events, independent of the festival', '5',
    'Other seasonal factors and events could explain the higher bookings. The festival may contribute, but the data do not clearly isolate its effect.',
    'Causal Reasoning'
WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE questiontext = 'Which answer best explains why the festival may not be the real cause of higher hotel bookings?');