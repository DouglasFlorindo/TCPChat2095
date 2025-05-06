using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace TCPChatGUI.Connection;
public static class NetworkUtils
{
    public static IPAddress GetLocalIPv4()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Ppp ||
                ni.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase))
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (!IPAddress.IsLoopback(addr.Address) &&
                        !addr.Address.ToString().StartsWith("169.254."))
                    {
                        return addr.Address;
                    }
                }
            }
        }
        throw new Exception("IP not found.");
    }
}