from flask import Flask, request, jsonify, render_template
import requests
import json

app = Flask(__name__)

with open('settings.json') as config_file:
    config = json.load(config_file)

received_messages = []

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/send', methods=['POST'])
def send_data():
    message = request.form['message']
    data_to_send = {'message': message}
    response = requests.post(config['target_endpoint'], json=data_to_send)
    return jsonify({'status': 'sent', 'response': response.text})

@app.route('/callback', methods=['POST'])
def callback():
    data = request.json
    received_messages.append(data)  
    return jsonify({'status': 'success', 'data_received': True})

@app.route('/fetch-callback-data', methods=['GET'])
def fetch_callback_data():
    return jsonify(received_messages)

if __name__ == '__main__':
    app.run(debug=True)
