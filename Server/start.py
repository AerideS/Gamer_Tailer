import socket 
import joblib

# 모델 불러오기
model = joblib.load('./model_data.pkl')

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 8080)
server_socket.bind(server_address)
server_socket.listen(1)

while True:
    client_socket, client_address = server_socket.accept()
    print('{}에서 접속이 확인되었습니다.'.format(client_address))

    # 모델로 예측하기
    my_data = [[19.11170196533203, 0.20000000298023224, 31, 0, 2]]
    print(model.predict(my_data))

    while True:
        data = client_socket.recv(1024)
        if not data:
            break
        client_socket.send(data)

    client_socket.close()

server_socket.close()