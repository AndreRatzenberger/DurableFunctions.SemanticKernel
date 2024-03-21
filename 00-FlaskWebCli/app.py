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
    headers = {'Content-Type': 'text/plain'}
    response = requests.post(config['target_endpoint'], data=message, headers=headers)
    return response.text
    # data_to_send = {'message': message}
    # response = requests.post(config['target_endpoint'], json=data_to_send)
    # return jsonify({'status': 'sent', 'response': response.text})

@app.route('/callback', methods=['POST'])
def callback():
    data = request.data.decode('utf-8') 
    received_messages.append(data)  
    return 'Success', 200
    # data = request.json
    # received_messages.append(data)  
    # return jsonify({'status': 'success', 'data_received': True})

@app.route('/fetch-callback-data', methods=['GET'])
def fetch_callback_data():
    return jsonify(received_messages)

@app.route('/clear', methods=['DELETE'])
def clear():
    received_messages.clear()
    return 'Success', 200

if __name__ == '__main__':
    app.run(debug=True)
