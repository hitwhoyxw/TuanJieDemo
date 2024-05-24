using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class StartUp : MonoBehaviour
{
    NetClient netClient;
    private MainView mainView;
    TextMeshPro txtIP;
    TextMeshPro txtPort;
    void Start()
    {
        netClient = NetClient.Instance;
        mainView=GetComponent<MainView>();
        txtIP = mainView.txt_ip.GetComponent<TextMeshPro>();
        txtPort = mainView.txt_port.GetComponent<TextMeshPro>();

        mainView.btn_localConnect.onClick.AddListener(OnLocalConnectClicked);
        mainView.btn_remoteConnect.onClick.AddListener(OnRemoteConnectClicked);
    }
    private void Awake()
    {
        
    }
    private void OnDestroy()
    {
        mainView.btn_localConnect.onClick.RemoveListener(OnLocalConnectClicked);
        mainView.btn_remoteConnect.onClick.RemoveListener(OnRemoteConnectClicked);
    }
    private void OnLocalConnectClicked()
    {
        netClient.StartConnect();
        
    }
    private void OnRemoteConnectClicked()
    {
        string ip = txtIP.text;
        string port = txtPort.text;
        int.TryParse(port, out int portInt);
        netClient.StartConnect("127.0.0.1", 55011);
    }
}
