using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Threading;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using testMailValidate.data;
using testMailValidate.services;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Reflection.Metadata;
using System.Windows;
using MotorDetection.RealDetection.ViewModels;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot;

namespace tcp_client_controller.ViewModel
{

    public class OrderModel
    {


        public int Id { get; set; }
    public string Name { get; set; }
        public string Header { get; set; }
        public int ParameterNumber { get; set;  }
        public string Parameter { get;set; }
        
        
        
        
    }


    public class ClientHelperViewModel : BindableBase
    {
      public string IPAddr { get; set;  }  = "192.168.1.11";
      public string Port { get; set;  } = "8088";

      public string tcp_send_buff = string.Empty;

      bool is_connect;
      public bool IsConnect { get => is_connect; set => SetProperty(ref is_connect,value); }

      /// <summary>
      ///  
      /// </summary>
      public bool IsListening
      {
          get => _isListening;
          set => SetProperty(ref _isListening, value);
      }


      public string TCP_Send_Buff
        {
            get => tcp_send_buff;
            set => SetProperty(ref tcp_send_buff, value);
        }

        string TCP_receive_buff = string.Empty;
        public string TCP_Receive_Buff
        {
            get => TCP_receive_buff;
            set => SetProperty(ref TCP_receive_buff, value);
        }
        
        
        private int  send_frame_num =0;

        public int SendFrameNum 
        {
            get => send_frame_num;
            set => SetProperty(ref send_frame_num, value);
        }

        int  recv_frame_num =0;

        public int RecvFrameNum
        {
            get => recv_frame_num;
            set =>SetProperty(ref recv_frame_num, value);
        }

        // string OSC_Add;
        //  string tcp_send_buff;



        public bool IsApplyCycleSend { get; set; }
        public bool IsHex { get; set; } = true;
        public bool IsCRC16 { get; set; } = true;
        public string  OSC_CycleTime { get; set; } = "1";

        private int osc_move_vel = 10; 
        public int OSC_Move_Vel { 

            get=> osc_move_vel ;
            set=>SetProperty(ref osc_move_vel,value);
        }


        private string coord = "0,0";
        public string Coord 
        {
            get => coord;
            set => SetProperty(ref coord, value);
        }

        public int OSC_Step { get; set; } = 1000; 

        public string Motor_CycleTime { get; set; } = "1";
        public string SendTime { get; set; } = "0";
        private DispatcherObject _dispatcher;
        private SqlUtil<OrderModel> sqlHelper;

        //private Socket client;
        public ClientHelperViewModel(DispatcherObject dispatcher)
        {
            _dispatcher = dispatcher;
            Orders = new ObservableCollection<OrderModel>();
            sqlHelper = new SqlUtil<OrderModel>(new UserContext<OrderModel>());
            Orders.Clear();
            var task =sqlHelper.GetAllUsersAsync();
            task.Wait();
            Orders.AddRange(task.Result);
            ValueTuplePlotModel = CreateValueTupleGraph();
        }

        private DelegateCommand _connectCommand;
        public DelegateCommand ConnectCommand =>_connectCommand ??= new DelegateCommand(connect);

        private DelegateCommand _stopListenCommand;
        public DelegateCommand StopListenCommand => _stopListenCommand ??= new DelegateCommand(StopListen);


        private DelegateCommand _disconnectCommand;
        public DelegateCommand DisconnectCommand =>_disconnectCommand??= new DelegateCommand(disconnect);

        private DelegateCommand _sendCommand;
        public DelegateCommand SendCommand => _sendCommand??=new DelegateCommand(() => msg_send(TCP_Send_Buff));

        private DelegateCommand _oscCommand;
        public DelegateCommand OSC_SendCommand => _oscCommand??=new DelegateCommand(osc_send);

        private DelegateCommand _osc_changeCommand;
        public DelegateCommand OSC_ChangeVelCommand => _osc_changeCommand??=new DelegateCommand(change_osc_vel);
        
        
        public DelegateCommand OSC_GetVelCommand => new DelegateCommand(get_osc_vel);
        public DelegateCommand OSC_GetCoordCommand => new DelegateCommand(get_osc_coord);
        public DelegateCommand OSC_GotoCoordCommand => new DelegateCommand(goto_osc_coord);
        public DelegateCommand MotorSendCommand => new DelegateCommand(motor_send);
        public DelegateCommand SendClearCommand => new DelegateCommand(() => { 
            TCP_Send_Buff    = string.Empty;
        
        });

        public DelegateCommand RecvClearCommand => new DelegateCommand(() => { TCP_Receive_Buff = string.Empty; });


        private TcpListener listener;
        private TcpClient client; 

        public async void connect()
        {
            // client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPAddress ip_addr = IPAddress.Parse(IPAddr);
            //IPEndPoint ip_end = new IPEndPoint(ip_addr, int.Parse(Port));

             listener = new TcpListener(IPAddress.Parse(IPAddr), int.Parse(Port));

            // 开始监听客户端连接
            
            IsConnect = await Task.Run( async () => {
                try
                {
                    listener.Start();
                    IsListening = true;
                    // 接受一个客户端连接
                     client = await listener.AcceptTcpClientAsync();
                        Console.WriteLine("客户端已连接，IP地址：" + ((IPEndPoint)client.Client.RemoteEndPoint).Address);
                        
                    return true;
                }
                catch (Exception e)
                {
                    
                    Console.Write("Fail to connect server");
                    Console.Write(e.ToString());
                    return false;
                }
            });
           if(IsConnect)
                await recv_msg();
            

        }



        public async void StopListen()
        {
            listener.Stop();
            IsListening = false;
        }


        public async void disconnect()
        {
             await Task.Run(() =>
            {
                try
                {
                    client.Close();
                    IsConnect = false;
                    listener.Stop();
                    IsListening = false;
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }
        byte[] buffer = new byte[1024];
        private ObservableCollection<OrderModel> _orders;
        private int _selectIndex=-1;

        public async Task recv_msg()
        {
         IsConnect=await Task.Run(() =>
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                 
                    try
                    {
                        while (true)
                        {
                            
                            // 获取客户端的网络流，用于读取和写入数据
                            
                            int totalRead=0;
                            int bytesRead;

                            // 从客户端接收和处理数据
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalRead += bytesRead; 
                            }

                            
                            
                            
                            StringBuilder strB = new StringBuilder();
                            for (int i = 0; i <totalRead; i++)

                            {

                                strB.Append(buffer[i].ToString("X2")+" ");
                              
                            }
                            
                            
                            
                            //TCP_Receive_Buff += strB.ToString()+'\n';
                            RecvFrameNum ++;
                        }
                      
                        
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                        return false;
                    }

                }
            }
                );
        }

        
        private void send_msg(byte[] buffer)
        {

            try
            {
                
                client.GetStream().Write(buffer, 0, buffer.Length);
                SendFrameNum++;
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        
        }

        private bool StopSendFlag = false;
        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand=>_stopCommand??=new DelegateCommand(()=>StopSendFlag=true);

        
        
        
        /// <summary>
        ///  ascii码发送和hex的发送
        /// 
        /// </summary>
        /// <param name="tcpSendBuff"></param>
        public async void msg_send(string tcpSendBuff)
        {
            byte[] send = null;

            if (!IsHex)
                send = Encoding.ASCII.GetBytes(tcpSendBuff);
            else
            {
                try
                {
                    send = tcpSendBuff.Split(' ', ',', '-').Select(str => byte.Parse(str, System.Globalization.NumberStyles.HexNumber)).ToArray();
                }
                catch
                {
                    System.Windows.MessageBox.Show("请输入正确的十六进制数,使用',',空格和'-'来分隔");
                    return;
                }
            }
            byte[] crc_send = null;
            /*   make new crc send    */
            if (IsCRC16)
            {
                crc_send = new byte[send.Length + 2];
                send.CopyTo(crc_send, 0);
                ushort temp = FrameCheck.CRC16_Check_T(crc_send, send.Length);
                crc_send[send.Length] = (byte)(temp >> 8 & 0xff);
                crc_send[send.Length + 1] = (byte)(temp & 0xff);
            }
            else crc_send = send;

            if (IsApplyCycleSend)
            {
                int sendtime = int.Parse(SendTime);
                int time = int.Parse(OSC_CycleTime);
                if (sendtime > 0)
                {
                    await Task.WhenAny(
                         Task.Run(() =>
                         {
                             try
                             {
                                 while (IsApplyCycleSend && !StopSendFlag)
                                 {
                                     send_msg(crc_send);
                                     Thread.Sleep(time);
                                 }
                             }
                             catch (SocketException e)
                             {
                                 System.Windows.MessageBox.Show(e.Message);
                                 return;
                             }

                         }),
                       Task.Delay(sendtime)
                        );
                }
                else await Task.Run(() =>
                {
                    while (IsApplyCycleSend && !StopSendFlag)
                    {
                        send_msg(crc_send);
                        Thread.Sleep(int.Parse(OSC_CycleTime));
                    }

                });
            }
            else
                send_msg(crc_send);

            StopSendFlag = false; 

        }



        #region 指令模式

        public ObservableCollection<OrderModel> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }


        public int SelectIndex
        {
            get => _selectIndex;
            set => SetProperty(ref _selectIndex, value);
        }


        public async void SaveItem(object model)
        {
            if (model is not OrderModel sel) return;
                //if (SelectIndex == -1) return;
                //var sel = Orders[SelectIndex];
                if (!await sqlHelper.UpdateUserAsync(sel,sel.Id, (old, update ) =>
                {
                    old.Header = update.Header;
                    old.Parameter = update.Parameter;
                    old.Name = update.Name;
                    old.ParameterNumber=update.ParameterNumber;
                }))
            await sqlHelper.AddUserAsync(sel);
        }

        public async void AddItem()
        {
            var order = new OrderModel() { Name = "" };
            if (SelectIndex == -1)
            {
                int count = Orders.Count;
                Orders.Add(order);
            }
            else
                Orders.Insert(SelectIndex+1, order);

            

            //for (int i = 0; i < Orders.Count; ++i)
            //{
            //    Orders[i].Number = i + 1;
            //}
        }

        


        public async void DeleteItem()
        {
            if (SelectIndex == -1)
                return;

            await sqlHelper.DeleteUserAsync(Orders[SelectIndex].Id);
            Orders.RemoveAt(SelectIndex);
            
            
            /*for (int i = 0; i < Orders.Count; ++i)
            {
                Scenes[i].Number = i + 1;
            }*/
        }

        public async void SendOrder(object model)
        {
            if (model is not OrderModel cur) return;
            
            
            msg_send(cur.Header+" "+cur.ParameterNumber+cur.Parameter);
        }


        private DelegateCommand _addOrderCommand;
        private DelegateCommand _deleteOrderCommand;
        private DelegateCommand<object> _saveOrderCommand;
        private DelegateCommand<object> _sendOrderCommand;
        
        
        public DelegateCommand AddOrderCommand => _addOrderCommand ??= new DelegateCommand(AddItem);
        public DelegateCommand DeleteOrderCommand => _deleteOrderCommand ??= new DelegateCommand(DeleteItem);
        public DelegateCommand<object> SaveOrderCommand => _saveOrderCommand ??= new DelegateCommand<object>(SaveItem);
public DelegateCommand<object>SendOrderCommand => _sendOrderCommand ??=new DelegateCommand<object>(SendOrder);

        #endregion


        #region  绘图



        /// <summary>
        /// 值类型
        /// </summary>
        public enum ValueMode
        {
            Time,
            Value
        }

        private ValueMode _xMode;
        public ValueMode XMode
        {
            get => _xMode;
            set => SetProperty(ref _xMode, value);
        }


        private ValueMode _yMode;

        public ValueMode YMode
        {
            get => _yMode;
            set => SetProperty(ref _yMode, value);
        }


        private DelegateCommand _ApplyCommand;
        public DelegateCommand ApplyCommand => _ApplyCommand ??= new DelegateCommand(Apply);

        private void Apply()
        {
            ModifyPlotModelAxisUnit(ValueTuplePlotModel, 0, XMode == ValueMode.Time ? _timeTitle : XTitle);
            ModifyPlotModelAxisUnit(ValueTuplePlotModel, 1, YMode == ValueMode.Time ? _timeTitle : YTitle);
        }


        private readonly string _timeTitle = "ms";
        private string _xTitle;
        private string _yTitle;
        public string XTitle
        {
            get => _xTitle;
            set => SetProperty(ref _xTitle, value);
        }

        public string YTitle
        {
            get => _yTitle;
            set => SetProperty(ref _yTitle, value);
        }



        private ViewResolvingPlotModel _valueTuplePlotModel;
        private bool _isListening;


        public ViewResolvingPlotModel ValueTuplePlotModel
        {
            get => _valueTuplePlotModel;
            set => SetProperty(ref _valueTuplePlotModel, value);
        }

        /// <summary>
        ///  值组曲线生成
        /// </summary>
        /// <returns></returns>
        private ViewResolvingPlotModel CreateValueTupleGraph()
        {

            //create plotModel
            var plotModel = new ViewResolvingPlotModel()
            {
                DefaultFont = "微软雅黑"
            };
            //add legends
            var l = new Legend
            {
                LegendFontSize = double.NaN,
                LegendTitle = "",
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendBorder = OxyColors.Black,
                GroupNameFont = "Segoe UI Black",
                LegendPosition = LegendPosition.TopLeft,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal,
                AllowUseFullExtent = true
            };

            plotModel.Legends.Add(l);


            //add axis_x : time 
            var axisX = new LinearAxis
            {

                Position = AxisPosition.Bottom, //x-axis
                //  PositionAtZeroCrossing = true,  
                // Minimum = 0,
                // Maximum = 12000,
                AbsoluteMinimum = 0,
                // AbsoluteMaximum = 12000,
                //AxisTitleDistance = 20,
                TicklineColor = OxyColors.Blue,
                MinorTicklineColor = OxyColors.Gray,
                AxislineStyle = LineStyle.Solid,
                TickStyle = OxyPlot.Axes.TickStyle.Crossing,
                Title = "X"
            };
            plotModel.Axes.Add(axisX);

            // add  axisY_voltage .voltage series
            var axisY = new LinearAxis
            {

                Position = AxisPosition.Left, //y -axis 
                //   PositionAtZeroCrossing = true,
                /*Minimum =0 ,
                Maximum = 3,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 3,*/
                //AxisTitleDistance = 20,
                TicklineColor = OxyColors.Blue,
                MinorTicklineColor = OxyColors.Gray,
                AxislineStyle = LineStyle.Solid,
                TickStyle = OxyPlot.Axes.TickStyle.Crossing,
                //PositionTier = 1,
                Title = "Y"
            };
            plotModel.Axes.Add(axisY);

            return plotModel;
        }

        protected void ModifyPlotModelAxisUnit(ViewResolvingPlotModel plotModel, int index, string title)
        {
            plotModel.Axes[index].Title = title;
            plotModel.InvalidatePlot(false);
        }



        #endregion



        #region  硬件指令发送

        public async void motor_send()
        {

            byte[] send = { 0xa1, 0x02, 0x01 ,0x01, 0x42 ,0x48 };
            if (IsApplyCycleSend)
            {
                int time = int.Parse(Motor_CycleTime);
                await Task.Run(() =>
                    {
                      
                        try
                        {
                            while (IsApplyCycleSend)
                            {
                                 send_msg(send);
                                 Thread.Sleep(time);
                               
                            }
                        }
                        catch (SocketException e)
                        {
                            System.Windows.MessageBox.Show(e.Message);
                            return;
                        }
                    }
                );
            }
            else
                send_msg(send);

        }

        public async void change_osc_vel()
        {
            byte[] osc = { 0xa2, 0x01, 0x02, 0x00, 0x00, 0, 0 };
            await Task.Run(() =>
            {
                try
                {

                   
                        osc[3] = (byte)(OSC_Move_Vel >> 8 & 0xff);
                        osc[4] = (byte)(OSC_Move_Vel & 0xff);
                        
                        ushort temp = FrameCheck.CRC16_Check_T(osc, 5);
                        osc[5] = (byte)(temp >> 8 & 0xff);
                        osc[6] = (byte)(temp & 0xff);
                        send_msg(osc);
                }
                catch (SocketException e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                    return;
                }
            });
        }

        public async void get_osc_vel()
        {
            byte[] osc = { 0xa2, 0x02, 0x00, 0, 0 };
            await Task.Run(() =>
            {
                try
                {

                    ushort temp = FrameCheck.CRC16_Check_T(osc, 3);
                    osc[3] = (byte)(temp >> 8 & 0xff);
                    osc[4] = (byte)(temp & 0xff);
                    send_msg(osc);
                    Thread.Sleep(10);
                    OSC_Move_Vel = buffer[3] << 8 | buffer[4];
                }
                catch (SocketException e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                    return;
                }
            });

        }
        public async void get_osc_coord()
        {
            byte[] osc = { 0xa2, 0x03, 0x00, 0, 0 };

            await Task.Run(() =>
            {
                try
                {

                    ushort temp = FrameCheck.CRC16_Check_T(osc, 3);
                    int x = 0, y = 0;
                    osc[3] = (byte)(temp >> 8 & 0xff);
                    osc[4] = (byte)(temp & 0xff);
                    send_msg(osc);
                    Thread.Sleep(10);
                    x = buffer[4] << 8 | buffer[5];
                    y = buffer[7] << 8 | buffer[8];
                    if (buffer[3] == 1)
                        x = -x;
                    if (buffer[6] == 1)
                        y = -y;
                    Coord = x.ToString() + "," + y.ToString();
                }
                catch (SocketException e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                    return;
                }
            });

        }
        
       public async void goto_osc_coord()
        {
            byte[] osc = { 0xa2, 0x00, 0x07, 0x00, 0x03, 0xe8, 0x00, 0x03, 0xe8, 0x01, 0, 0 };
            string[] xy_coord = Coord.Split(',',' ','，');
            int x = int.Parse(xy_coord[0]);
            int y = int.Parse(xy_coord[1]);
            await Task.Run(() =>
            {
                try
                {
                 

                        osc[3] = (byte)(x < 0 ? 0x01 : 0x00);
                        osc[4] = (byte)(Math.Abs(x) >> 8 & 0xff);
                        osc[5] = (byte)(Math.Abs(x) & 0xff);
                        osc[6] = (byte)(y < 0 ? 0x01 : 0x00);
                        osc[7] = (byte)(Math.Abs(y) >> 8 & 0xff);
                        osc[8] = (byte)(Math.Abs(y) & 0xff);
                        ushort temp = FrameCheck.CRC16_Check_T(osc, 10);
                        osc[10] = (byte)(temp >> 8 & 0xff);
                        osc[11] = (byte)(temp & 0xff);
                        send_msg(osc);
            
                }
                catch (SocketException e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                    return;
                }
            });

        }

        public async void osc_send()
        {
          
            byte[] osc = { 0xa2, 0x00, 0x07,0x00, 0x03, 0xe8,0x00, 0x03, 0xe8, 0x01,0, 0 };
            int x = 0; int y = 0;
            bool is_stop = false;
            if (IsApplyCycleSend)
            {
                int sendtime = int.Parse(SendTime);
                int time = int.Parse(OSC_CycleTime);
                if (sendtime > 0)
                {
                    await Task.WhenAny(Task.Run(() =>
                {

                    try
                    {
                        bool dir = false;
                        while (IsApplyCycleSend)
                        {
                            if (is_stop) break;
                            if (x >= 32000)
                                dir = true;
                            if (x <= -32000)
                                dir = false;
                            if (dir)
                            {
                                x -= OSC_Step;
                                y -= OSC_Step;
                            }
                            else
                            {
                                x += OSC_Step;
                                y += OSC_Step;
                            }
                        
                            osc[3] = (byte)(x < 0 ? 0x01 : 0x00);
                            osc[4] = (byte)(Math.Abs(x) >> 8 & 0xff);
                            osc[5] = (byte)(Math.Abs(x) & 0xff);
                            osc[6] = (byte)(y < 0 ? 0x01 : 0x00);
                            osc[7] = (byte)(Math.Abs(y) >> 8 & 0xff);
                            osc[8] = (byte)(Math.Abs(y) & 0xff);
                            ushort temp = FrameCheck.CRC16_Check_T(osc, 10);
                            osc[10] = (byte)(temp >> 8 & 0xff);
                            osc[11] = (byte)(temp & 0xff);
                            send_msg(osc);
                            Thread.Sleep(time); 

                        }
                    }
                    catch (SocketException e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                        return;
                    }
                }), Task.Delay(sendtime));
                    is_stop = true;
                } else
                    await Task.Run(() =>
                   {
                       int stime = int.Parse(OSC_CycleTime);
                       try
                       {
                           bool dir = false;
                           while (IsApplyCycleSend)
                           {
                               if (is_stop) break;
                               if (x >= 32000)
                                   dir = true;
                               if (x <= -32000)
                                   dir = false;
                               if (dir)
                               {
                                   x -= OSC_Step;
                                   y -= 0;
                               }
                               else
                               {
                                   x += OSC_Step;
                                   y += 0;
                               }

                               osc[3] = (byte)(x < 0 ? 0x01 : 0x00);
                               osc[4] = (byte)(Math.Abs(x) >> 8 & 0xff);
                               osc[5] = (byte)(Math.Abs(x) & 0xff);
                               osc[6] = (byte)(y < 0 ? 0x01 : 0x00);
                               osc[7] = (byte)(Math.Abs(y) >> 8 & 0xff);
                               osc[8] = (byte)(Math.Abs(y) & 0xff);
                               ushort temp = FrameCheck.CRC16_Check_T(osc, 10);
                               osc[10] = (byte)(temp >> 8 & 0xff);
                               osc[11] = (byte)(temp & 0xff);
                               send_msg(osc);
                               Thread.Sleep(time);

                           }
                       }
                       catch (SocketException e)
                       {
                           System.Windows.MessageBox.Show(e.Message);
                           return;
                       }
                   });
            }
            else
                send_msg(osc);
           
        }
#endregion
    }
}
