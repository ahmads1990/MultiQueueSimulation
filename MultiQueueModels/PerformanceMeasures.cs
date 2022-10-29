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
            int totalTime = 0;
            List<int> times = new List<int>();
            int currentTime = 0;
            for (int i = 0; i < SimulationTable.Count; i++)
            {
                if (SimulationTable[i].TimeInQueue!=0)
                {
                    int queue = 1;
                    int currentIndex = i;
                    currentTime=SimulationTable[i].StartTime;
                    while (currentIndex + 1 < SimulationTable.Count)
                    {
                        if (SimulationTable[++currentIndex].ArrivalTime >= currentTime)
                        {
                            break;
                        }
                        queue++;
                    }           
                    times.Add(queue);
                }
            }
            MaxQueueLength = times.Max();
        }

        public void probOfIdleServer(ref List<Server> servers,int TotalTime, int serverId, int totalCustomer)
        {
            decimal probability = 0;
            /*
             *  probability = ((decimal)servers[serverId-1].TotalWorkingTime) / (decimal)TotalTime;
            
            servers[serverId - 1].IdleProbability = 1m-probability;
             */
            probability = ((decimal)TotalTime-(decimal)servers[serverId-1].TotalWorkingTime) / (decimal)TotalTime;
            
            servers[serverId - 1].IdleProbability = probability;
            servers[serverId - 1].AverageServiceTime = (decimal)servers[serverId - 1].TotalWorkingTime
                / (decimal)servers[serverId-1].serviceCount;
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

