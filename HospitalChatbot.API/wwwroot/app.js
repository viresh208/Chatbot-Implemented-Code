const API_BASE_URL = window.location.origin + '/api';

let sessionId = null;
let isWaitingForResponse = false;

const chatContainer = document.getElementById('chatContainer');
const userInput = document.getElementById('userInput');
const sendBtn = document.getElementById('sendBtn');
const startBtn = document.getElementById('startBtn');
const inputArea = document.getElementById('inputArea');

startBtn.addEventListener('click', startChat);
sendBtn.addEventListener('click', sendMessage);
userInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter' && !isWaitingForResponse) {
        sendMessage();
    }
});

async function startChat() {
    try {
        const response = await fetch(`${API_BASE_URL}/chatbot/start`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Failed to start chat session');
        }

        const data = await response.json();
        sessionId = data.sessionId;

        startBtn.style.display = 'none';
        inputArea.style.display = 'flex';

        chatContainer.innerHTML = '';

        const initialResponse = await sendMessageToBot('');

        userInput.focus();
    } catch (error) {
        console.error('Error starting chat:', error);
        addBotMessage('Sorry, there was an error starting the chat. Please refresh the page and try again.');
    }
}

async function sendMessage() {
    const message = userInput.value.trim();

    if (!message || isWaitingForResponse) {
        return;
    }

    addUserMessage(message);
    userInput.value = '';

    await sendMessageToBot(message);
}

async function sendMessageToBot(message) {
    isWaitingForResponse = true;
    sendBtn.disabled = true;
    userInput.disabled = true;

    showTypingIndicator();

    try {
        const response = await fetch(`${API_BASE_URL}/chatbot/message`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                sessionId: sessionId,
                message: message
            })
        });

        if (!response.ok) {
            throw new Error('Failed to send message');
        }

        const data = await response.json();

        removeTypingIndicator();

        addBotMessage(data.message, data.options);

        if (data.isCompleted) {
            userInput.disabled = true;
            sendBtn.disabled = true;
            setTimeout(() => {
                addBotMessage('Would you like to start a new booking? Please refresh the page.');
            }, 2000);
        }

    } catch (error) {
        console.error('Error sending message:', error);
        removeTypingIndicator();
        addBotMessage('Sorry, there was an error processing your message. Please try again.');
    } finally {
        isWaitingForResponse = false;
        sendBtn.disabled = false;
        userInput.disabled = false;
        userInput.focus();
    }
}

function addUserMessage(message) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message user';
    messageDiv.innerHTML = `
        <div class="message-content">${escapeHtml(message)}</div>
    `;
    chatContainer.appendChild(messageDiv);
    scrollToBottom();
}

function addBotMessage(message, options = null) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message bot';

    const formattedMessage = formatMessage(message);

    messageDiv.innerHTML = `
        <div class="bot-icon">🤖</div>
        <div class="message-content">${formattedMessage}</div>
    `;

    chatContainer.appendChild(messageDiv);

    if (options && options.length > 0) {
        addOptions(options);
    }

    scrollToBottom();
}

function addOptions(options) {
    const optionsDiv = document.createElement('div');
    optionsDiv.className = 'message bot';

    let optionsHtml = '<div class="bot-icon" style="opacity: 0;"></div><div class="options-container">';

    options.forEach(option => {
        optionsHtml += `
            <button class="option-btn" onclick="selectOption('${escapeHtml(option.value)}', '${escapeHtml(option.display)}')">
                ${escapeHtml(option.display)}
            </button>
        `;
    });

    optionsHtml += '</div>';
    optionsDiv.innerHTML = optionsHtml;

    chatContainer.appendChild(optionsDiv);
    scrollToBottom();
}

function selectOption(value, display) {
    const allOptionButtons = document.querySelectorAll('.option-btn');
    allOptionButtons.forEach(btn => btn.disabled = true);

    addUserMessage(display);
    sendMessageToBot(value);
}

function showTypingIndicator() {
    const typingDiv = document.createElement('div');
    typingDiv.className = 'typing-indicator';
    typingDiv.id = 'typingIndicator';
    typingDiv.innerHTML = `
        <div class="bot-icon">🤖</div>
        <div class="typing-content">
            <div class="typing-dots">
                <span></span>
                <span></span>
                <span></span>
            </div>
        </div>
    `;
    chatContainer.appendChild(typingDiv);
    scrollToBottom();
}

function removeTypingIndicator() {
    const typingIndicator = document.getElementById('typingIndicator');
    if (typingIndicator) {
        typingIndicator.remove();
    }
}

function formatMessage(message) {
    return escapeHtml(message).replace(/\n/g, '<br>');
}

function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

function scrollToBottom() {
    chatContainer.scrollTop = chatContainer.scrollHeight;
}
