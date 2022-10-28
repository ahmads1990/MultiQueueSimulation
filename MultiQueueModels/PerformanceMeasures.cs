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
        public void CalculatePerformanceMeasures(ref List<SimulationCase> SimulationTable)
        {
            CalculateAverageWaitingTime(ref SimulationTable);
            CalculateWaitingProbability(ref SimulationTable);
            CalculateMaxQueueLength(ref SimulationTable);
        }
        public void CalculateAverageWaitingTime(ref List<SimulationCase> SimulationTable)
        {
            int total_time_customer_waited = 0;
            for (int i = 0; i < SimulationTable.Count; i++)
            {
                total_time_customer_waited += SimulationTable[i].TimeInQueue;
            }
            AverageWaitingTime = ((decimal)total_time_customer_waited / (decimal)SimulationTable.Count);
        }
        public decimal CalculateWaitingProbability(ref List<SimulationCase> SimulationTable)
        {

            int counter = 0;
            for (int i = 0; i < SimulationTable.Count; i++)
            {
                if (SimulationTable[i].TimeInQueue > 0)
                {
                    counter++;
                }
            }
            WaitingProbability = (decimal)counter / (decimal)SimulationTable.Count;
            return WaitingProbability;
        }
        // max queue lentgh
        public void CalculateMaxQueueLength(ref List<SimulationCase> SimulationTable)
        {
            int max = 0;
            foreach (var simCase in SimulationTable)
            {
                if (simCase.TimeInQueue > max)
                {
                    max = simCase.TimeInQueue;
                }
            }
            MaxQueueLength = max;
        }

        public decimal probOfIdleServer(ref Server serverID, ref SimulationSystem totalTime)
        {
            decimal probability = 0;
            probability = serverID.ID / totalTime.TotalTime;
            return probability;
        }
        public decimal averageServiceTime(ref List<SimulationCase> SimCase){
            decimal average = 0;
            int total_time_Service = 0;
            for (int i = 0; i < SimCase.Count; i++)
            {
                total_time_Service += SimCase[i].ServiceTime;
            }
            average = (decimal)total_time_Service / (decimal)SimCase.Count;
            return average;
            }
    }
}

