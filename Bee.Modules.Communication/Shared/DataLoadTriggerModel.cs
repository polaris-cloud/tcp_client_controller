using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Series;

namespace Bee.Modules.Communication.Shared
{
    public class Container
    {
        public int Exist;
        public int Remain;
        public int Capacity;
    }

    public struct  Loader
    {
        public int Length;
        public int Remain;
    }

    public class DataLoadTriggerModel
    {
         Container container;
         Loader loader;

         bool changed = false;
        public DataLoadTriggerModel(int capacity, ViewResolvingPlotModel model, LineSeries line,Dispatcher dispatcher)
         {
             this.model = model;
             this.line = line;
             _dispatcher = dispatcher;
             container=new Container(){Exist = 0 ,Remain = capacity,Capacity = capacity};
                 containerBuffer= new byte[container.Capacity];
            model.Updated += (s, e) =>
            {
                changed = true;
            };
        }

         private ViewResolvingPlotModel model;
         private LineSeries line;
         private readonly Dispatcher _dispatcher;

         private int nowProcess;
         public void Load(byte[] data)
         {
             Loader loader = new Loader(){Length = data.Length,Remain = data.Length};
             byte[] containerBuffer =new byte[loader.Length];
             do
             {
                 int loadBytes = container.Remain > loader.Remain ? loader.Remain : container.Remain;
                 int start = loader.Length - loader.Remain;
                 //byte[] range = new byte[loadBytes];
                 Array.Copy(data, start, containerBuffer, container.Exist, loadBytes);
                 
                 container.Exist += loadBytes;
                 container.Remain = container.Capacity - container.Exist;
                 loader.Remain -= loadBytes;
                 
                 if (container.Remain == 0 && changed)
                 {
                     
                    changed = false;
                    updataPlot();
                    clearPlot();
                     container.Exist = 0;
                     container.Remain = container.Capacity;
                     //nowProcess += container.Capacity; 
                     
                 }
                 
             } while (loader.Remain > 0); 
         }

         private byte[] containerBuffer;
        public void LoadSpecificBytes(byte[] data)
        {

                Loader loader = new Loader() { Length = data.Length, Remain = data.Length };
                
            do
                {
                    int loadBytes = container.Remain > loader.Remain ? loader.Remain : container.Remain;
                    int start = loader.Length - loader.Remain;
                //byte[] range = new byte[loadBytes];
                Array.Copy(data, start, containerBuffer, container.Exist, loadBytes);

                
                    container.Exist += loadBytes;
                    container.Remain = container.Capacity - container.Exist;
                    loader.Remain -= loadBytes;
                    
                    if (container.Remain == 0 && changed)
                    {
                    for (int i = 0; i < container.Capacity; i += 2)
                    {
                        ushort vol = (ushort)((containerBuffer[i] << 8) | containerBuffer[i+1]);
                        var vol_1 = vol * 3.3 / 4096;
                          line.Points.Add(new DataPoint(nowProcess+i/2, vol_1));
                    }
                    //reset container
                    updataPlot();
                        clearPlot();
                        container.Exist = 0;
                        container.Remain = container.Capacity;
                        //nowProcess += container.Capacity; 
                        //}else if (loader.Remain == 0)
                        //{
                        //    updataPlot();
                        //}
                    }
                } while (loader.Remain > 0);
            
            
        }



        public void updataPlot()
         {
             model.InvalidatePlot(false);
         }

         public void clearPlot()
         {
             line.Points.Clear();
            //model.InvalidatePlot(true);
        }

         public void ResetProcess()
         {
             nowProcess = 0;
         }

    }
}
