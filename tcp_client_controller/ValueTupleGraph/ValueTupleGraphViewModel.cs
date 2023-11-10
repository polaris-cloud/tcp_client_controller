using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MotorDetection.RealDetection.ViewModels;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot;
using Prism.Mvvm;
using System.Windows.Threading;
using Prism.Commands;

namespace tcp_client_controller.ValueTupleGraph
{
public class ValueTupleGraphViewModel:BindableBase
{
    private DispatcherObject _dispatcherObject;
        public ValueTupleGraphViewModel(DispatcherObject dispatcher)
        {
            _dispatcherObject = dispatcher;
        }



        
    }
}
