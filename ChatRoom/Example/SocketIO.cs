using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;
using BestHTTP.Examples;
using System;

public class SocketIO : MonoBehaviour {

    private SocketManager Manager;
    // Use this for initialization
    void Start()
    {


        // Change an option to show how it should be done
        SocketOptions options = new SocketOptions();
        options.AutoConnect = false;

        // Create the Socket.IO manager
        Manager = new SocketManager(new Uri("http://localhost:3000/socket.io/"), options);

        // Set up custom chat events
        //Manager.Socket.On("chat message", OnChat);
        //Manager.Socket.On("new message", OnNewMessage);
        //Manager.Socket.On("user joined", OnUserJoined);
        //Manager.Socket.On("user left", OnUserLeft);
        //Manager.Socket.On("typing", OnTyping);
        //Manager.Socket.On("stop typing", OnStopTyping);

        // The argument will be an Error object.
        //Manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));

        //Manager.GetSocket("/").On(SocketIOEventTypes.Connect, (socket, packet, arg) =>
        //{
        //    Debug.LogWarning("Connected to /nsp");

        //    socket.Emit("testmsg", "Message from /nsp 'on connect'");
        //});

        //Manager.GetSocket("/nsp").On("nsp_message", (socket, packet, arg) => {
        //    Debug.LogWarning("nsp_message: " + arg[0]);
        //});

        // We set SocketOptions' AutoConnect to false, so we have to call it manually.
        
    }

    private void OnChat(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("zzzzz");
        Debug.Log(args[0].ToString());
    }

    private void OnApplicationQuit()
    {
        Manager.Close();
    }


    private void OnGUI()
    {
        if (GUILayout.Button("Connect",GUILayout.Width(50), GUILayout.Height(50)))
        {
            Manager.Open();
            Manager.Socket.On("chat message", OnChat);
        }

        if (GUILayout.Button("Disconnect", GUILayout.Width(50), GUILayout.Height(50)))
        {
            Manager.Socket.Off("chat message");
            Manager.Close();
        }
    }
}
