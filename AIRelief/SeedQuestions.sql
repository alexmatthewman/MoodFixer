-- Seed questions extracted from commit 3df40eb (aireliefdb_old.db)
-- Run this script against aireliefdb.db to restore the original questions.
-- Safe to re-run: each INSERT is guarded by a NOT EXISTS check on the maintext.

INSERT INTO Questions (
    heading, maintext, image, backvalue, nextvalue,
    explanationtext, explanationimage,
    Option1, Option2, Option3, Option4, Option5,
    CorrectAnswer, Category
)
SELECT
    NULL,
    'A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?',
    'q1.png', NULL, NULL,
    'At first glance, you might think the eraser costs $0.50 since the pencil costs 50 cents more. But, if the pencil costs $0.75 and the eraser costs $0.25, together they add up to $1.00. The first instinct is often to assume the eraser costs more because of how the question is framed, but the correct breakdown is $0.25 for the eraser and $0.75 for the pencil.',
    'q1x.png',
    '$0.50', '$0.25', '$0.75', '$0.05', NULL,
    '$0.25', 'Trial'
WHERE NOT EXISTS (
    SELECT 1 FROM Questions
    WHERE maintext = 'A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?'
);

INSERT INTO Questions (
    heading, maintext, image,
    explanationtext, explanationimage,
    Option1, Option2, Option3, Option4, Option5,
    CorrectAnswer, Category
)
SELECT
    NULL,
    'In a race, you pass the person in second place. What place are you in now?',
    'q2.png',
    'It''s easy to think you''re now in 1st place, but if you pass the person in second place, you''re now in 2nd, not 1st. The person in 1st is still ahead of you.',
    'q2x.png',
    '2nd', '1st', '3rd', '4th', NULL,
    '2nd', 'Trial'
WHERE NOT EXISTS (
    SELECT 1 FROM Questions
    WHERE maintext = 'In a race, you pass the person in second place. What place are you in now?'
);

INSERT INTO Questions (
    heading, maintext, image,
    explanationtext, explanationimage,
    Option1, Option2, Option3, Option4, Option5,
    CorrectAnswer, Category
)
SELECT
    NULL,
    'If a train leaves New York at 10:00 AM and travels at 60 miles per hour, and another train leaves the same station at the same time but travels at 90 miles per hour, how long will it take before the second train catches up to the first?',
    NULL,
    'This is a trick question. The second train is faster, but both trains are leaving from the same station at the same time. They''ll never catch up because they''re already on the same path, just traveling at different speeds.',
    'q3x.png',
    '1 hour', '30 minutes', 'They will never meet', '2 hours', NULL,
    'They will never meet', 'Trial'
WHERE NOT EXISTS (
    SELECT 1 FROM Questions
    WHERE maintext = 'If a train leaves New York at 10:00 AM and travels at 60 miles per hour, and another train leaves the same station at the same time but travels at 90 miles per hour, how long will it take before the second train catches up to the first?'
);

INSERT INTO Questions (
    heading, maintext, image,
    explanationtext, explanationimage,
    Option1, Option2, Option3, Option4, Option5,
    CorrectAnswer, Category
)
SELECT
    NULL,
    'A car travels 30 miles in 30 minutes. What is the average speed of the car?',
    'q4.png',
    'The car travels 30 miles in 30 minutes, which is the same as 0.5 hours. So, the average speed is 30 miles / 0.5 hours = 60 miles per hour. The trick is in interpreting the time properly and not letting the "30 minutes" confuse you.',
    NULL,
    '60 miles per hour', '30 miles per hour', '15 miles per hour', '2 miles per minute', NULL,
    '60 miles per hour', 'Trial'
WHERE NOT EXISTS (
    SELECT 1 FROM Questions
    WHERE maintext = 'A car travels 30 miles in 30 minutes. What is the average speed of the car?'
);

