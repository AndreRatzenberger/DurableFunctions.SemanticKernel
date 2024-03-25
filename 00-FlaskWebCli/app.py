from flask import Flask, request, jsonify, render_template
from flask_socketio import SocketIO, emit
import requests
import json

app = Flask(__name__)
socketio = SocketIO(app)

with open('settings.json') as config_file:
    config = json.load(config_file)

received_messages = []
newData = False

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/send', methods=['POST'])
def send_data():
    message = request.form['message']
    headers = {'Content-Type': 'text/plain'}
    response = requests.post(config['target_endpoint'], data=message, headers=headers)
    return response.text

@app.route('/callback', methods=['POST'])
def callback():
    data = request.data.decode('utf-8') 
    received_messages.append(data)
    socketio.emit('callback_data', received_messages) 
    return 'Success', 200

@socketio.on('connect')
def handle_connect():
    print('Client connected')

@socketio.on('disconnect')
def handle_disconnect():
    print('Client disconnected')

@socketio.on('fetch_data')
def handle_fetch_data():
    emit('callback_data', received_messages)

@app.route('/clear', methods=['DELETE'])
def clear():
    received_messages.clear()
    return 'Success', 200

if __name__ == '__main__':
    socketio.run(app, debug=True)
