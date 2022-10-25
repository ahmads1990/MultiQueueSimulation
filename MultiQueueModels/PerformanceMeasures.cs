using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiQueueModels
{
    public class PerformanceMeasures
    {
        public decimal AverageWaitingTime { get; set; }
        public int MaxQueueLength { get; set; }
        public decimal WaitingProbability { get; set; }

        //---------------------------------------------------------
        public decimal Avareg_wating_time(ref SimulationSystem simulation_system, ref PerformanceMeasures avargewatingtime)
        {
            int total_time_customer_waited = 0;
            for (int i = 0; i < simulation_system.SimulationTable.Count; i++)
            {
                total_time_customer_waited += simulation_system.SimulationTable[i].TimeInQueue;
            }
            avargewatingtime.AverageWaitingTime = total_time_customer_waited / simulation_system.SimulationTable.Count;
            return avargewatingtime.AverageWaitingTime;
        }

        public decimal waiting_probability(ref SimulationSystem simulation_system, ref PerformanceMeasures waiting_probability)
        {

            int counter = 0;
            for (int i = 0; i < simulation_system.SimulationTable.Count; i++)
            {
                if (simulation_system.SimulationTable[i].TimeInQueue > 0)
                {
                    counter++;
                }
            }
            waiting_probability.WaitingProbability = counter / simulation_system.SimulationTable.Count;
            return waiting_probability.WaitingProbability;
        }
        // max queue lentgh


    }
}

