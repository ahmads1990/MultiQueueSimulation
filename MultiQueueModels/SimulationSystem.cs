using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiQueueModels
{
    public class SimulationSystem
    {
        public SimulationSystem()
        {
            this.Servers = new List<Server>();
            this.InterarrivalDistribution = new List<TimeDistribution>();
            this.PerformanceMeasures = new PerformanceMeasures();
            this.SimulationTable = new List<SimulationCase>();
        }

        ///////////// INPUTS ///////////// 
        public int NumberOfServers { get; set; }
        public int StoppingNumber { get; set; }
        public List<Server> Servers { get; set; }
        public List<TimeDistribution> InterarrivalDistribution { get; set; }
        public Enums.StoppingCriteria StoppingCriteria { get; set; }
        public Enums.SelectionMethod SelectionMethod { get; set; }

        ///////////// OUTPUTS /////////////
        public List<SimulationCase> SimulationTable { get; set; }
        public PerformanceMeasures PerformanceMeasures { get; set; }


        public void DistributionTimeCustomer(List<int> inter_arrival_time, List<decimal> probability)
        {
            int customerCount = inter_arrival_time.Count;
            //for first customer
            InterarrivalDistribution.Add(new TimeDistribution());
            InterarrivalDistribution[0].Time = inter_arrival_time[0];
            InterarrivalDistribution[0].Probability = probability[0];
            InterarrivalDistribution[0].CummProbability = InterarrivalDistribution[0].Probability;
            InterarrivalDistribution[0].MinRange = 1;
            InterarrivalDistribution[0].MaxRange = (int)(InterarrivalDistribution[0].CummProbability * 100);
            for (int i = 1; i < customerCount; i++)
            {
                InterarrivalDistribution.Add(new TimeDistribution());

                InterarrivalDistribution[i].Time = inter_arrival_time[i];
                InterarrivalDistribution[i].Probability = probability[i];
                InterarrivalDistribution[i].CummProbability = InterarrivalDistribution[i - 1].CummProbability + InterarrivalDistribution[i].Probability;
                InterarrivalDistribution[i].MinRange = (InterarrivalDistribution[i - 1].MaxRange) + 1;
                InterarrivalDistribution[i].MaxRange = ((int)(InterarrivalDistribution[i].CummProbability * 100));
            }
        }

        public void DistributionTimeService(int index_server, List<int> serviceTime, List<decimal> probability)
        {
            
            Servers[index_server].TimeDistribution.Add(new TimeDistribution());

            int serviceCount = serviceTime.Count;
            Servers[index_server].TimeDistribution[0].Time = serviceTime[0];
            Servers[index_server].TimeDistribution[0].Probability = probability[0];
            Servers[index_server].TimeDistribution[0].CummProbability = Servers[index_server].TimeDistribution[0].Probability;
            Servers[index_server].TimeDistribution[0].MinRange = 1;
            Servers[index_server].TimeDistribution[0].MaxRange = (int)(Servers[index_server].TimeDistribution[0].CummProbability * 100);
            for (int i = 1; i < serviceCount; i++)
            {
                Servers[index_server].TimeDistribution.Add(new TimeDistribution());
                Servers[index_server].TimeDistribution[i].Time = serviceTime[i];
                Servers[index_server].TimeDistribution[i].Probability = probability[i];
                Servers[index_server].TimeDistribution[i].CummProbability = Servers[index_server].TimeDistribution[i - 1].CummProbability + Servers[index_server].TimeDistribution[i].Probability;
                Servers[index_server].TimeDistribution[i].MinRange = (Servers[index_server].TimeDistribution[i - 1].MaxRange) + 1;
                Servers[index_server].TimeDistribution[i].MaxRange = ((int)(Servers[index_server].TimeDistribution[i].CummProbability * 100));
            }


        }

        private int whichRange(int value, List<TimeDistribution> list_of_distribution)
        {
            
            int count_list = list_of_distribution.Count;
            for (int i = 0; i < count_list; i++)
            {
                if (value >= list_of_distribution[i].MinRange && value <= list_of_distribution[i].MaxRange)
                    return list_of_distribution[i].Time;
            }
            return 0;
        }

    }
}
