import socket 

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 8080)
server_socket.bind(server_address)
server_socket.listen(1)

while True:
    client_socket, client_address = server_socket.accept()
    print('{}에서 접속이 확인되었습니다.'.format(client_address))

    while True:
        data = client_socket.recv(1024)
        if not data:
            break
        client_socket.send(data)

    client_socket.close()

server_socket.close()