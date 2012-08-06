using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace DelcomTests
{
    public struct packetStruct
    {
        public byte Recipient;
        public byte DeviceModel;
        public byte MajorCmd;
        public byte MinorCmd;
        public byte DataLSB;
        public byte DataMSB;
        public short Length;   
    }
   

    public class Program
    {
        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x0FC5, 0x1223);

        // Setup the colours
        private const int GREEN = 0x01;
        private const int RED = 0x02;
        private const int ORANGE = 0x04;

        private static UsbEndpointReader endpointReader;

        static void Main(string[] args)
        {
            ErrorCode errorCode = ErrorCode.None;

            


            try
            {
                UsbRegDeviceList regList = UsbDevice.AllDevices.FindAll(MyUsbFinder);
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                UsbSetupPacket packet = new UsbSetupPacket(
                    (byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                    (byte)0x12,
                    (short)0xc8,
                    (0x02 * 0x100) + 0x0a,
                    0);
                var buffer = new byte[] { 0x65, 0x0C, 0x02, 0xFF, 0x00, 0x00, 0x00, 0x00 };

                UsbInterfaceInfo usbInterfaceInfo = null;
                UsbEndpointInfo usbEndpointInfo = null;
                byte recipient = (byte)UsbRequestType.TypeVendor;
                var rubyPacket = new UsbSetupPacket(recipient, 0x0A, (byte)0x0c, 0x04, 0x01);
                
               // UsbEndpointBase.LookupEndpointInfo(MyUsbDevice.Configs[0], MyUsbDevice.ActiveEndpoints[0].EndpointInfo,
                 //                                  out usbInterfaceInfo, out usbEndpointInfo);
                int temp3;
                var RubyTransfer = MyUsbDevice.ControlTransfer(ref rubyPacket, null, 0, out temp3);
                // 0x21, 0x09, 0x0635, 0x000, "\x65\x0C#{data}\xFF\x00\x00\x00\x00", 0

                //int temp1;
                //var result1 = MyUsbDevice.ControlTransfer(ref packet, new byte[2], 8, out temp1);

                //packet = new UsbSetupPacket((byte)UsbRequestType.TypeVendor, (byte)0x12, (short)0x0C0A, (short)0x0200, 0x000);
                //int temp2;
                //var result2 = MyUsbDevice.ControlTransfer(ref packet, null, 0, out temp2);

                Console.WriteLine(string.Format("Result 1 : {0} - Length: {1}", RubyTransfer, temp3));
                //Console.WriteLine(string.Format("Result 2 : {0}", result2));

                //usb_control_msg(led->udev,
                //      usb_sndctrlpipe(led->udev, 0),
                //      0x12,
                //      0xc8,
                //      (0x02 * 0x100) + 0x0a,
                //      (0x00 * 0x100) + color,
                //      buffer,
                //      8,
                //      2 * HZ);

                // Then turn the LED on
                //Packet.Recipient = 8;
                //Packet.DeviceModel = 18;
                //Packet.Length = 0;
                //Packet.MajorCmd = 10;
                //Packet.MinorCmd = 12;
                ////switch (Color)
                ////{
                ////    case GREENLED: Packet.DataLSB = 1; break;
                ////    case REDLED: Packet.DataLSB = 2; break;
                ////    case BLUELED: Packet.DataLSB = 4; break;
                ////}
                //Packet.DataLSB = 4; // blue
                //Packet.DataMSB = 0;


            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            } finally
            {
                if (MyUsbDevice != null)
                {
                    if(endpointReader != null)
                        endpointReader.Abort();
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }
            }

            Console.ReadKey();
        }

        private static void ReaderOnDataReceived(object sender, EndpointDataEventArgs endpointDataEventArgs)
        {
            Console.WriteLine("Data received on ep01.");
            Console.WriteLine(endpointDataEventArgs.Buffer);
        }

        private static void endpointReader_DataReceived(object sender, EndpointDataEventArgs e)
        {
            Console.WriteLine(e.Buffer);
        }
    }
}
