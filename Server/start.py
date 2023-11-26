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

    while True:
        data = client_socket.recv(1024)
        if not data:
            print("No data received or empty data.") 
            break
        print(type(data))
        print("Received data:", data)

        # 데이터를 쉼표(,)로 분리하여 각 숫자를 정수로 변환
        numbers_as_strings = data.decode('utf-8').split(', ')
        
        # 문자열을 숫자형태로 바궈서 저장
        try:
            stageIndex = int(numbers_as_strings[0])
            avgEpvLevel = float(numbers_as_strings[1])
            avgAliveTime = float(numbers_as_strings[2])
            avgHitCount = int(numbers_as_strings[3])
            totaltryCount = int(numbers_as_strings[4])
        except ValueError as e:
            print("Error parsing values", e)

        print("stageIndex:", stageIndex)
        print("avgEpvLevel:", avgEpvLevel)
        print("avgAliveTime:", avgAliveTime)
        print("avgHitCount:", avgHitCount)
        print("totaltryCount:", totaltryCount)
        
        # 모델로 예측하기
        # 'PavgAliveTime0', 'PavgEpvLevel0', 'PavgHitCount0','PstageIndex0', 'PtotalTryCount0'
        predicts = model.predict([[avgAliveTime, avgEpvLevel, avgHitCount, stageIndex, totaltryCount]])
        print(predicts)

        # 클라이언트로 예측 결과 전송
        response = str(predicts) # 예측 결과를 문자열로 변환
        client_socket.sendall(response.encode())

server_socket.close()