using System;
using System.Collections.Generic;
using System.Collections; 
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

        Random r_s_id = new Random();
        Random r_s = new Random();


        public void startSimulation() { }
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
        private void calculate_service_time(ref SimulationCase next_customer)
        {

            next_customer.RandomService = r_s.Next(1, 100);
            int used_index = (next_customer.AssignedServer.ID) - 1;
            int l = Servers[used_index].TimeDistribution.Count;

            for (int i = 0; i < l; i++)
            {

                if (next_customer.RandomService >= Servers[next_customer.AssignedServer.ID - 1].TimeDistribution[i].MinRange &&
                    next_customer.RandomService <= Servers[next_customer.AssignedServer.ID - 1].TimeDistribution[i].MaxRange)
                {

                    next_customer.ServiceTime = Servers[next_customer.AssignedServer.ID - 1].TimeDistribution[i].Time;
                    break;
                }

            }

            next_customer.EndTime = next_customer.ServiceTime + next_customer.StartTime;
        }
        private void check_Priority(ref SimulationCase next_customer)
        {
            List<int> Currently_available = new List<int>();
            for (int i = 0; i < Servers.Count; ++i)
            {
                bool foundService = false;

                for (int j = SimulationTable.Count - 1; j >= 0; j--)
                {
                    if (SimulationTable[j].AssignedServer.ID == i + 1)
                    {
                        foundService = true;
                        if (next_customer.ArrivalTime >= SimulationTable[j].EndTime)
                            Currently_available.Add(i + 1);

                        break;
                    }
                }
                if (!foundService)
                    Currently_available.Add(i + 1); // the server is free

            }

            if (Currently_available.Count == 1)
            {
                next_customer.TimeInQueue = 0;
                next_customer.StartTime = next_customer.ArrivalTime;
                next_customer.AssignedServer.ID = Currently_available[0];
                calculate_service_time(ref next_customer);
                Servers[next_customer.AssignedServer.ID - 1].TotalWorkingTime += next_customer.ServiceTime;
            }
            else if (Currently_available.Count == 0) //search for the nearset server will be available   
            {
                int min_diffrence = 2000000000;
                int min_ID = 1000000000;
                List<int> Nearset_will_be_available = new List<int>();
                for (int k = 0; k < Servers.Count; ++k)
                {
                    for (int i = SimulationTable.Count - 1; i >= 0; i--)
                    {
                        if (SimulationTable[i].AssignedServer.ID == k + 1)
                        {
                            if (SimulationTable[i].EndTime - next_customer.ArrivalTime < min_diffrence)
                            {
                                min_diffrence = SimulationTable[i].EndTime - next_customer.ArrivalTime;
                                min_ID = SimulationTable[i].AssignedServer.ID;
                            }
                            break;
                        }

                    }
                }

                Nearset_will_be_available.Add(min_ID);

                for (int k = 0; k < Servers.Count; k++)
                {
                    for (int i = SimulationTable.Count - 1; i >= 0; i--)
                    {
                        if (k + 1 == SimulationTable[i].AssignedServer.ID)
                        {
                            if (SimulationTable[i].EndTime - next_customer.ArrivalTime == min_diffrence &&
                                SimulationTable[i].AssignedServer.ID != min_ID)
                            {
                                Nearset_will_be_available.Add(SimulationTable[i].AssignedServer.ID);
                            }
                            break;
                        }

                    }
                }

                Nearset_will_be_available.Sort();
                next_customer.TimeInQueue = min_diffrence;
                next_customer.StartTime = next_customer.ArrivalTime + next_customer.TimeInQueue;  // get the time in the queue and the start time of the service  

                if (Nearset_will_be_available.Count == 1)
                    next_customer.AssignedServer.ID = Nearest_will_be_available[0];
                calculate_service_time(ref next_customer);
                Servers[next_customer.AssignedServer.ID - 1].TotalWorkingTime += next_customer.ServiceTime;

                else if (Nearset_will_be_available.Count > 1)
                {
                    Selection_methods(ref next_customer, Nearset_will_be_available);
                }
            }

            else if (Currently_available.Count > 1)
            {
                next_customer.TimeInQueue = 0; // No wait TimeInQueue 
                next_customer.StartTime = next_customer.ArrivalTime;// start time = the arrival time 
                Selection_methods(ref next_customer, Currently_available);
            }

        }
        private void Selection_methods(ref SimulationCase next_customer, List<int> server)
        {
            // priority Selcetion Method   
            if (SelectionMethod == Enums.SelectionMethod.HighestPriority)
                select_HighestPriority(ref next_customer, server);

            //random genertate  
            else if (SelectionMethod == Enums.SelectionMethod.Random)
                select_Random(ref next_customer, server);

            // utilization  
            else if (SelectionMethod == Enums.SelectionMethod.LeastUtilization)
                select_LeastUtilization(ref next_customer, server);
        }



        private void select_HighestPriority(ref SimulationCase next_customer, List<int> server)
        {
            next_customer.AssignedServer.ID = server[0];
            calculate_service_time(ref next_customer);

            Servers[next_customer.AssignedServer.ID - 1].TotalWorkingTime += next_customer.ServiceTime;
        }



        private void select_Random(ref SimulationCase next_customer, List<int> server) //changed
        {
            next_customer.AssignedServer.ID = server[r_s_id.Next(0, server.Count)]; //-1
            calculate_service_time(ref next_customer);
            Servers[next_customer.AssignedServer.ID - 1].TotalWorkingTime += next_customer.ServiceTime;

        }



        private void select_LeastUtilization(ref SimulationCase next_customer, List<int> server)
        {
            int min_ID = Servers[server[0] - 1].ID;
            int min_Work = Servers[server[0] - 1].TotalWorkingTime;
            for (int i = 1; i < server.Count; ++i)
            {

                if (Servers[server[i] - 1].TotalWorkingTime < min_Work)
                {
                    min_Work = Servers[server[i] - 1].TotalWorkingTime;
                    min_ID = Servers[server[i] - 1].ID;
                }
            }
            next_customer.AssignedServer.ID = min_ID;
            calculate_service_time(ref next_customer);
            Servers[next_customer.AssignedServer.ID - 1].TotalWorkingTime += next_customer.ServiceTime;
        }

        /////SIMULATION TABLE//////////
        //go through each customer and make call function MakeRow
        public List<SimulationCase> MakeTable(int NoCusts, List<int> Servers)
        {
            List<SimulationCase> totalTable = new List<SimulationCase>();
            for (int i = 0; i < NoCusts; i++)
            {
                totalTable.Add(MakeRow(i, Servers));
            }
            return totalTable;

        }
        public SimulationCase MakeRow(int CustNo, List<int> Servers )
        {
            Random rn = new Random();
            SimulationCase sm = new SimulationCase();
            sm.CustomerNumber = CustNo;
            if (CustNo == 1)
            {
                sm.ArrivalTime = 0;
                sm.RandomInterArrival = 0;
                sm.InterArrival = 0;
                sm.RandomService = rn.Next(1, 100);


            }
            else { sm.RandomInterArrival = rn.Next(1, 100); }
            //random serveice time
            sm.RandomService = rn.Next(1, 100);
            select_Random(ref sm, Servers);
            //select server
             Selection_methods(ref sm, Servers);
            check_Priority(ref sm);
           //calculate service time
           
            calculate_service_time(ref sm);
            //time in qeueu
            sm.TimeInQueue = sm.InterArrival - sm.StartTime;
            return sm;

        }
    }
}
