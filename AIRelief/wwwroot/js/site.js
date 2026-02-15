// Theme switching functionality
document.addEventListener('DOMContentLoaded', function () {
    // Get footer theme toggle
    const footerToggle = document.getElementById('footer-theme-toggle');

    // Get current theme from localStorage or default to dark
    const currentTheme = localStorage.getItem('theme') || 'dark';
    document.documentElement.setAttribute('data-theme', currentTheme);
    updateToggleIcon(currentTheme);

    // Theme toggle event listener
    function toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateToggleIcon(newTheme);
    }

    if (footerToggle) {
        footerToggle.addEventListener('click', toggleTheme);
    }

    function updateToggleIcon(theme) {
        // Update footer toggle
        if (footerToggle) {
            const icon = footerToggle.querySelector('.theme-icon');
            const text = footerToggle.querySelector('.theme-text');

            if (theme === 'dark') {
                icon.textContent = '☀️';
                text.textContent = 'Light Mode';
            } else {
                icon.textContent = '🌙';
                text.textContent = 'Dark Mode';
            }
        }
    }
});

// Trial Assessment JavaScript Functions

// Sound effects initialization
let correctSound, incorrectSound;
let isAnswering = false;
let currentQuestionIndex = 0;
let totalQuestions = 3;

// Initialize sounds when document is ready
$(document).ready(function () {
    try {
        correctSound = new Howl({
            src: ['/sounds/correct.mp3', '/sounds/correct.wav'],
            volume: 0.5
        });

        incorrectSound = new Howl({
            src: ['/sounds/incorrect.mp3', '/sounds/incorrect.wav'],
            volume: 0.5
        });
    } catch (e) {
        console.log('Howler.js not available, sounds disabled');
    }

    // Initialize progress indicator
    updateProgressIndicator();
});

function selectAnswer(selectedAnswer, element) {
    console.log('Answer selected:', selectedAnswer);

    if (isAnswering) return;
    isAnswering = true;

    // Get question ID from multiple possible sources
    const questionId = window.currentQuestionId ||
        $('#question-container').data('question-id') ||
        parseInt($('input[name="questionId"]').val()) ||
        parseInt($('.question-card').data('question-id'));

    console.log('Using question ID:', questionId);

    // Disable all answer buttons
    $('.answer-option').prop('disabled', true);

    // Submit answer
    $.post('/Trial/SubmitAnswer', {
        questionId: questionId,
        selectedAnswer: selectedAnswer
    })
        .done(function (response) {
            console.log('Full response received:', response);
            console.log('Correction text from response:', response.correctionText);
            console.log('Correction image from response:', response.correctionImage);

            if (response.success) {
                if (response.isCorrect) {
                    handleCorrectAnswer(element, response);
                } else {
                    handleIncorrectAnswer(element, response);
                }
            } else {
                alert('Error: ' + response.message);
                resetAnswering();
            }
        })
        .fail(function (xhr, status, error) {
            console.error('AJAX Error:', status, error);
            console.error('Response Text:', xhr.responseText);
            alert('Network error. Please try again.');
            resetAnswering();
        });
}

function resetAnswering() {
    isAnswering = false;
    $('.answer-option').prop('disabled', false);
}

function handleCorrectAnswer(element, response) {
    // Flash green
    $(element).addClass('correct-flash');

    // Play sound if available
    try {
        if (correctSound) {
            correctSound.play();
        }
    } catch (e) {
        console.log('Sound not available');
    }

    // Show positive feedback
    setTimeout(function () {
        showFeedback(true, function () {
            proceedToNext(response);
        });
    }, 1000);
}

function handleIncorrectAnswer(element, response) {
    console.log('Handling incorrect answer with correction:', response.correctionText);

    // Flash red
    $(element).addClass('incorrect-flash');

    // Play sound if available
    try {
        if (incorrectSound) {
            incorrectSound.play();
        }
    } catch (e) {
        console.log('Sound not available');
    }

    // Show correction with a slight delay
    setTimeout(function () {
        // Pass the response data directly to ensure fresh data
        showCorrection(response.correctionText, response.correctionImage, function () {
            proceedToNext(response);
        });
    }, 1000);
}

// ADD THIS MISSING FUNCTION
function proceedToNext(response) {
    console.log('proceedToNext called with response:', response);

    if (response.isLastQuestion) {
        console.log('Last question reached, redirecting to:', response.nextQuestionUrl);
        window.location.href = response.nextQuestionUrl;
    } else {
        console.log('Loading next question...');
        currentQuestionIndex++;
        updateProgressIndicator();
        loadNextQuestion();
    }
}

function showFeedback(isPositive, callback) {
    // Remove any existing overlays first
    $('.feedback-overlay').remove();

    const feedbackHtml = `
        <div class="feedback-overlay">
            <div class="feedback-content positive-feedback">
                <div class="feedback-icon success-icon">✅</div>
                <h3>Excellent!</h3>
                <p>That's the correct answer.</p>
                <button class="btn btn-primary" onclick="closeFeedback()" id="continueBtn">Continue</button>
            </div>
        </div>`;

    $('body').append(feedbackHtml);

    // Use flexbox display and fade in
    $('.feedback-overlay').css('display', 'flex').hide().fadeIn(300);

    $('#continueBtn').click(function () {
        console.log('Continue button clicked');
        closeFeedback();
        callback();
    });
}

function showCorrection(correctionText, correctionImage, callback) {
    console.log('showCorrection called with:', { correctionText, correctionImage });

    // Remove any existing overlays first
    $('.feedback-overlay').remove();

    // Update the image path to include QuestionImages folder
    const imageHtml = correctionImage ?
        `<img src="/images/QuestionImages/${correctionImage}" alt="Correction" class="correction-image" />` : '';

    // Ensure we have some correction text
    const displayText = correctionText || 'That answer was incorrect. Please review the material and try again.';

    const correctionHtml = `
        <div class="feedback-overlay">
            <div class="feedback-content correction-feedback">
                <div class="feedback-icon error-icon">❌</div>
                <h3>Not Quite Right</h3>
                ${imageHtml}
                <p>${displayText}</p>
                <button class="btn btn-primary" onclick="closeFeedback()" id="okBtn">OK</button>
            </div>
        </div>`;

    console.log('Appending correction HTML:', correctionHtml);

    $('body').append(correctionHtml);

    // Use flexbox display and fade in
    $('.feedback-overlay').css('display', 'flex').hide().fadeIn(300);

    $('#okBtn').click(function () {
        console.log('OK button clicked, calling callback');
        closeFeedback();
        callback();
    });
}

function closeFeedback() {
    console.log('closeFeedback called');
    $('.feedback-overlay').fadeOut(300, function () {
        $(this).remove();
    });
}

function loadNextQuestion() {
    console.log('loadNextQuestion called');

    $.get('/Trial/NextQuestion')
        .done(function (html) {
            console.log('Loading next question HTML:', html.substring(0, 200) + '...');

            $('#question-container').fadeOut(300, function () {
                $(this).html(html).fadeIn(300);

                // Update the question ID for the new question - multiple methods to ensure it works
                const $newContent = $(html);
                const newQuestionId = $newContent.find('.question-card').data('question-id') ||
                    $newContent.find('[data-question-id]').data('question-id') ||
                    $newContent.find('input[name="questionId"]').val();

                console.log('New question ID found:', newQuestionId);

                if (newQuestionId) {
                    window.currentQuestionId = parseInt(newQuestionId);
                    $('#question-container').data('question-id', newQuestionId);
                    $('.question-card').data('question-id', newQuestionId);
                }

                resetAnswering();
            });
        })
        .fail(function (xhr, status, error) {
            console.error('Error loading next question:', status, error);
            alert('Error loading next question.');
            resetAnswering();
        });
}

function updateProgressIndicator() {
    const questionNumber = currentQuestionIndex + 1;
    $('#current-question').text(questionNumber);
    const percentage = (questionNumber / totalQuestions) * 100;
    $('.progress-bar').css('width', percentage + '%');

    console.log('Progress updated:', questionNumber, 'of', totalQuestions, '(' + percentage + '%)');
}

// Trial-specific smooth scrolling
function initTrialScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Initialize trial functions when document is ready
$(document).ready(function () {
    initTrialScrolling();

    // Set initial question ID if available
    if (typeof questionId !== 'undefined') {
        window.currentQuestionId = questionId;
    }

    // Initialize progress
    currentQuestionIndex = 0;
    updateProgressIndicator();
});