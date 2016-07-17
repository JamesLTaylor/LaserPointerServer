import time
import zmq
import win32api, win32con
import socket as socketlib
import sys

ipaddress = socketlib.gethostbyname(socketlib.gethostname())
print("server running on:" + ipaddress)
server_name = "tcp://" + ipaddress + ":5555"
print("full name:" + server_name)

screen_cols = win32api.GetSystemMetrics(0)
screen_rows = win32api.GetSystemMetrics(1)
top_y = 5.0
bottom_y = 0.0
left_x = 4.0
right_x = -4.0

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind(server_name)

ctrl_down = False
mouse_left_down = False

"""
q - quit
xx,yy - set mouse to xx, yy
txx,yy - set top
bxx,yy - set bottom
lxx,yy - set left
rxx,yy - set right
cy - set ctrl and left button down to true
cn - set ctrl and left button down to false
pu - send page up
pd - send page down
dy - set just left button down to true
du - set just left button down to false

"""
while True:
    message = socket.recv()
    #print(message)
    if message=="q":
        print("Quitting")
        socket.send(b"q")
        break
    first_char = ord(message[0])   
    
    # default behaviour - set mouse position
    if first_char==45 or (first_char>=48 and first_char<=57):  
        (x, y) = message.split(",")
        x = float(x)
        y = float(y)
        pos_y = int((1 - (y-bottom_y) / (top_y-bottom_y))*screen_rows)
        pos_x = int((1 - (x-right_x) / (left_x-right_x))*screen_cols)    
        if mouse_left_down:
            win32api.mouse_event(win32con.MOUSEEVENTF_LEFTDOWN,pos_x,pos_y,0,0)
        win32api.SetCursorPos((pos_x, pos_y))
        socket.send(b"d") # d for done
        
    # Set the extent for top, bottom, left and right
    elif message[0]=="t":
        (x, y) = message[1:].split(",")
        x = float(x)
        y = float(y)
        top_y = y
        print("top_y set to " + str(top_y))
        socket.send(b"d") # d for done
    elif message[0]=="b":
        (x, y) = message[1:].split(",")
        x = float(x)
        y = float(y)
        bottom_y = y
        print("bottom_y set to " + str(bottom_y))
        socket.send(b"d") # d for done
    elif message[0]=="l":
        (x, y) = message[1:].split(",")
        x = float(x)
        y = float(y)
        left_x = x
        print("left_x set to " + str(left_x))
        socket.send(b"d") # d for done
    elif message[0]=="r":
        (x, y) = message[1:].split(",")
        x = float(x)
        y = float(y)
        right_x = x
        print("right_x set to " + str(right_x)) 
        socket.send(b"d") # d for done      
        
    # Ctrl and mouse down
    elif message[0]=="c":
        if len(message)<2:
            print("ERROR: valid codes starting with c are 'cy' and 'cn'")
            socket.send("ERROR: valid codes starting with c are 'cy' and 'cn'")
        elif message[1]=="y" and not ctrl_down:        
            win32api.keybd_event(win32con.VK_CONTROL, 0, 0, 0)
            ctrl_down = True
            mouse_left_down = True
            print("ctrl_down set to True")
            socket.send(b"d") # d for done            
        elif message[1]=="y" and ctrl_down:
            print("ctrl_down already True - no changes")
            socket.send(b"d") # d for done            
        elif message[1]=="n" and ctrl_down:        
            win32api.keybd_event(win32con.VK_CONTROL, 0, win32con.KEYEVENTF_KEYUP, 0)
            ctrl_down = False
            mouse_left_down = False
            print("ctrl_down set to False")
            socket.send(b"d") # d for done            
        elif message[1]=="n" and not ctrl_down:
            print("ctrl_down already False - no changes")            
            socket.send(b"d") # d for done            
        else:
            print("ERROR: valid codes starting with c are 'cy' and 'cn'")
            socket.send("ERROR: valid codes starting with c are 'cy' and 'cn'")

    # Page up and page down    
    elif message[0]=="p":
        error = False
        if ctrl_down:
            win32api.keybd_event(win32con.VK_CONTROL, 0, win32con.KEYEVENTF_KEYUP, 0)
        if len(message)<2:
            socket.send("ERROR: valid codes starting with p are 'pu' and 'pd'")
        elif message[1]=="d":
            win32api.keybd_event(win32con.VK_NEXT, 0,0,0)
            time.sleep(.1)
            win32api.keybd_event(win32con.VK_NEXT,0 ,win32con.KEYEVENTF_KEYUP ,0)
            print("page down sent")
        elif message[1]=="u":
            win32api.keybd_event(win32con.VK_PRIOR, 0,0,0)
            time.sleep(.1)
            win32api.keybd_event(win32con.VK_PRIOR,0 ,win32con.KEYEVENTF_KEYUP ,0)
            print("page up sent")
            
        else:
            error = True
            socket.send("ERROR: valid codes starting with p are 'pu' and 'pd'")
        
        if ctrl_down:
            win32api.keybd_event(win32con.VK_CONTROL, 0, 0, 0)
        if not error:
            socket.send(b"d") # d for done
            
    # Mouse left button up and down    
    elif message[0]=="d":
        error = False
        if len(message)<2:
            socket.send("ERROR: valid codes starting with p are 'dy' and 'dn'")
        elif message[1]=="y":
            mouse_left_down = True
            print("Left button set down")
            socket.send(b"d") # d for done
        elif message[1]=="n":
            win32api.mouse_event(win32con.MOUSEEVENTF_LEFTUP,pos_x,pos_y,0,0)
            mouse_left_down = False
            print("Left button set up")
            socket.send(b"d") # d for done
            
        else:
            socket.send("ERROR: valid codes starting with p are 'dy' and 'dn'")

    else:
        print("receiced unknown message: " + message) 
        socket.send(b"?") # ? for unknown
        

    
    