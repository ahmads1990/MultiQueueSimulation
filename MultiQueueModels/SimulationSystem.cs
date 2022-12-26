using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using System.IO;

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
            TotalTime = 0;
            totalCustomers = 0;
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

        Random randomNum = new Random();
        public int totalCustomers {get;set;}
        private int getRandom() { return randomNum.Next(1, 101); }
        /////SIMULATION TABLE//////////
        public void StartSimulation()
        {
            //Prepare Interarrival Distribution and Server Time  distribution 
            DistributionTimeCustomer();
            foreach (Server server in Servers)
            {
                DistributionTimeService(server.ID - 1);
            }
            //Start Making simulation table
            SimulationTable = MakeSimulationTable(StoppingNumber);
            //Calculate Performance Measures 
            List<SimulationCase> temp = SimulationTable;
            PerformanceMeasures.CalculatePerformanceMeasures(ref temp);
            List<Server> temp2 = Servers;
            foreach (var server in Servers)
            {
                PerformanceMeasures.probOfIdleServer(ref temp2, TotalTime, server.ID, totalCustomers);
            }
            //util
            foreach (var server in Servers)
            {
                server.Utilization = (decimal)server.TotalWorkingTime / (decimal)TotalTime;
                /*
                 *    Servers[newCustomer.AssignedServer.ID - 1].Utilization =
                (decimal)Servers[newCustomer.AssignedServer.ID - 1].TotalWorkingTime
                / (decimal)TotalTime;
                 * */
            }
        }
        //total time of services
        public int TotalTime { get; set; }
        public List<SimulationCase> MakeSimulationTable(int NoCusts)
        {
            List<SimulationCase> totalTable = new List<SimulationCase>();
            //add first customer as special case
            totalTable.Add(MakeRow(1, 0));
            totalCustomers++;
            //check stoppingCriteria then
            //go through each customer and make call function MakeRow
            if (StoppingCriteria == Enums.StoppingCriteria.NumberOfCustomers)
            {
                for (int i = 2; i <= NoCusts; i++)
                {
                    SimulationCase current = MakeRow(i, totalTable.Last().ArrivalTime);
                    totalTable.Add(current);
                    totalCustomers++;
                }
            }
            else
            {
                int count = 2;
                while (TotalTime <= StoppingNumber)
                {
                    SimulationCase current = MakeRow(count, totalTable.Last().ArrivalTime);

                    //CalcTime(current, totalTable.ElementAt(count - 1));
                    totalTable.Add(current);
                    totalCustomers++;
                    count++;
                }
            }
            /*
            if (totalTable.Last().EndTime > StoppingNumber
                &&StoppingCriteria==Enums.StoppingCriteria.SimulationEndTime)
            {
                totalTable.RemoveAt(totalTable.Count() - 1);
            }
            */
            return totalTable;
        }
        public SimulationCase MakeRow(int CustNo, int lastCustArrivalTime)
        {        
            SimulationCase sm = new SimulationCase();
            sm.CustomerNumber = CustNo;
            //First Customer
            if (CustNo == 1)
            {
                sm.RandomInterArrival = 1;
                sm.InterArrival = 0;
                sm.ArrivalTime = 0;
                //Select server
                Selector(ref sm);
                //Calculate service time
                CalculateServiceTime(ref sm);
            }
            else
            {
                sm.RandomInterArrival = getRandom();
                //mapping for random arrival time 
                sm.InterArrival = whichRange(sm.RandomInterArrival, InterarrivalDistribution);
                sm.ArrivalTime = sm.InterArrival + lastCustArrivalTime;
                /*
                if (CustNo == 3)
                {
                    Console.WriteLine();
                }
                */
                //Select server
                Selector(ref sm);

                //Calculate service time
                CalculateServiceTime(ref sm);
            }
            //TotalTime += sm.ServiceTime + sm.TimeInQueue;
            if (sm.EndTime > TotalTime)
            {
                TotalTime = sm.EndTime;
            }
            return sm;
        }
        public void CalcTime(SimulationCase currnt, SimulationCase last)
        {
            currnt.ArrivalTime = currnt.InterArrival + last.InterArrival;
        }
        //Prepare Interarrival Distribution
        public void DistributionTimeCustomer()
        {
            int tableLength = InterarrivalDistribution.Count;
            //for first customer
            InterarrivalDistribution[0].CummProbability = InterarrivalDistribution[0].Probability;
            InterarrivalDistribution[0].MinRange = 1;
            InterarrivalDistribution[0].MaxRange = (int)(InterarrivalDistribution[0].CummProbability * 100);
            for (int i = 1; i < tableLength; i++)
            {
                InterarrivalDistribution[i].CummProbability = InterarrivalDistribution[i - 1].CummProbability + InterarrivalDistribution[i].Probability;
                InterarrivalDistribution[i].MinRange = (InterarrivalDistribution[i - 1].MaxRange) + 1;
                InterarrivalDistribution[i].MaxRange = (int)(InterarrivalDistribution[i].CummProbability * 100);
            }
        }
        //Prepare Server Time  distribution 
        public void DistributionTimeService(int index_server)
        {
            int serviceCount = Servers[index_server].TimeDistribution.Count;
            Servers[index_server].TimeDistribution[0].CummProbability = Servers[index_server].TimeDistribution[0].Probability;
            Servers[index_server].TimeDistribution[0].MinRange = 1;
            Servers[index_server].TimeDistribution[0].MaxRange = (int)(Servers[index_server].TimeDistribution[0].CummProbability * 100);
            for (int i = 1; i < serviceCount; i++)
            {
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
        //Selection methods
        private void Selector(ref SimulationCase newCustomer)
        {
            switch (SelectionMethod)
            {
                case Enums.SelectionMethod.HighestPriority:
                    selectHighestPriority(ref newCustomer);
                    break;
                case Enums.SelectionMethod.Random:
                    selectRandomServer(ref newCustomer);
                    break;
                case Enums.SelectionMethod.LeastUtilization:
                    selectLeastUtilization(ref newCustomer);
                    break;
                default:
                    return;
            }
        }
        //1-return assigned server 2-time in queue
        //Highest Priority ,Priority = Index Order
        private void selectHighestPriority(ref SimulationCase newCustomer)
        {
            int serverId = 0;
            int nearestFreeTime = 10000000;
            //Search servers sequentially
            foreach (Server server in Servers)
            {
                if (server.FinishTime <= newCustomer.ArrivalTime)
                {
                    //found server //assign wait time
                    newCustomer.TimeInQueue = 0;
                    newCustomer.AssignedServer = server;
                    return;
                }
                else
                {
                    //Store to find nearest
                    if (server.FinishTime < nearestFreeTime)
                    {
                        serverId = server.ID;
                        nearestFreeTime = server.FinishTime;
                    }
                }
            }
            //all servers are busy
            newCustomer.TimeInQueue = nearestFreeTime - newCustomer.ArrivalTime;
            newCustomer.AssignedServer = Servers[serverId - 1];
        }
        //Random 
        private void selectRandomServer(ref SimulationCase newCustomer)
        {
            List<int> nearFree = new List<int>(Servers.Count);
            bool freeServers = false;
            foreach (var server in Servers)
            {
                nearFree[server.ID-1] = server.FinishTime;
                if (server.FinishTime <= newCustomer.ArrivalTime)
                {
                    freeServers = true;
                    break;
                }
            }
            if (freeServers)
            {
                //select Random
                while (true) {
                    int rn = randomNum.Next(0, Servers.Count);
                    if (Servers[rn].FinishTime <= newCustomer.ArrivalTime)
                    {
                        //found server //assign wait time
                        newCustomer.TimeInQueue = 0;
                        newCustomer.AssignedServer = Servers[rn];
                        break;
                    }
                }
            }
            else
            {
                //select near free
                int index = nearFree.IndexOf(nearFree.Min());
                //all servers are busy
                newCustomer.TimeInQueue = Servers[index].FinishTime - newCustomer.ArrivalTime;
                newCustomer.AssignedServer = Servers[index];
            }

        }
        //Least Utilization  
        private void selectLeastUtilization(ref SimulationCase newCustomer)
        {
            decimal leastUtil=1000M;
            int index=0;
            foreach (var server in Servers)
            {
                if (server.Utilization < leastUtil)
                {
                    leastUtil = server.Utilization;
                    index = server.ID - 1;
                }
            }
            newCustomer.AssignedServer = Servers[index];
            newCustomer.TimeInQueue = Servers[index].FinishTime - newCustomer.ArrivalTime;
        }
        //Assign Service start time end
        private void CalculateServiceTime(ref SimulationCase newCustomer)
        {
            int RandomService = getRandom();
            int serviceTime = whichRange(RandomService, newCustomer.AssignedServer.TimeDistribution);
            //Assign data
            newCustomer.RandomService = RandomService;
            newCustomer.StartTime = newCustomer.ArrivalTime + newCustomer.TimeInQueue;
            newCustomer.ServiceTime = serviceTime;
            newCustomer.EndTime = newCustomer.StartTime + newCustomer.ServiceTime;
            //Change Server Data
            //  newCustomer.AssignedServer.FinishTime = newCustomer.EndTime;
            //newCustomer.AssignedServer.TotalWorkingTime = newCustomer.AssignedServer.FinishTime;
            //
            Servers[newCustomer.AssignedServer.ID - 1].serviceCount += 1;
            Servers[newCustomer.AssignedServer.ID - 1].FinishTime = newCustomer.EndTime;
            Servers[newCustomer.AssignedServer.ID - 1].TotalWorkingTime += newCustomer.ServiceTime;
            if (TotalTime == 0)
            {
                Servers[newCustomer.AssignedServer.ID - 1].Utilization = 0M;
            }
            else
            {
                if (newCustomer.CustomerNumber == 50)
                {
                    Console.WriteLine();
                }
                
                Servers[newCustomer.AssignedServer.ID - 1].Utilization =
                    (decimal)Servers[newCustomer.AssignedServer.ID - 1].TotalWorkingTime
                    / (decimal)TotalTime;
            }
        }
        private int checkInteger(string input, int max, int min)
        {
            int result;
            if (Int32.TryParse(input, out result))
            {
                if (result > max && result < min) { return 0; }
                else return result;
            }
            else { return 0; }
        }

        public SimulationSystem ReadFile(string name)
        {
            int serversCount = 0;
            SimulationSystem simulationSystem = new SimulationSystem();
            bool textIsComplete = true;
            using (StreamReader reader = new StreamReader(name))
            {
                string line;
                while ((line = reader.ReadLine()) != null && textIsComplete)
                {
                    //check for number of servers
                    if (line.Equals("NumberOfServers"))
                    {
                        line = reader.ReadLine();
                        serversCount = checkInteger(line, 100, 1);
                        //simulationSystem.Servers = new List<Server>(serversCount);
                        if (serversCount <= 0) textIsComplete = false;

                    }
                    else if (line.Equals("StoppingNumber"))
                    {
                        line = reader.ReadLine();
                        int result = checkInteger(line, 1000, 1);
                        if (result > 0)
                            simulationSystem.StoppingNumber = result;
                        else
                            textIsComplete = false;
                    }
                    else if (line.Equals("StoppingCriteria"))
                    {
                        line = reader.ReadLine();
                        int result = checkInteger(line, 2, 1);
                        if (result > 0)
                            simulationSystem.StoppingCriteria = (Enums.StoppingCriteria)result;
                        else
                            textIsComplete = false;
                    }
                    else if (line.Equals("SelectionMethod"))
                    {
                        line = reader.ReadLine();
                        int result = checkInteger(line, 3, 1);
                        if (result > 0)
                            simulationSystem.SelectionMethod = (Enums.SelectionMethod)result;
                        else
                            textIsComplete = false;
                    }
                    else if (line.Equals("InterarrivalDistribution"))
                    {
                        while (true)
                        {
                            line = reader.ReadLine();
                            if (line == "") break;
                            string[] columns = line.Split(',');
                            simulationSystem.InterarrivalDistribution.Add(
                                new TimeDistribution
                                {
                                    Time = int.Parse(columns[0]),
                                    Probability = decimal.Parse(columns[1].Trim())
                                }
                                );
                        }
                    }
                    else if (line.Equals("ServiceDistribution_Server1"))
                    {
                        //read servers ServiceDistribution

                        int serverIndex = 1;
                        while (serverIndex <= serversCount)
                        {
                            Server n = new Server();
                            n.ID = serverIndex;
                            //test
                            while (true)
                            {
                                line = reader.ReadLine();
                                if (line == null) break;
                                if (line == "")
                                {
                                    line = reader.ReadLine();
                                    break;
                                }
                                string[] columns = line.Split(',');
                                TimeDistribution t = new TimeDistribution();
                                t.Time = (int.Parse(columns[0]));
                                t.Probability = decimal.Parse(columns[1].Trim());
                                n.TimeDistribution.Add(t);

                            }
                            simulationSystem.Servers.Add(n);
                            serverIndex++;
                        }
                    }
                }
            }
            return simulationSystem;
        }
        /*
        private void calculate_service_time(ref SimulationCase next_customer)
        {
            next_customer.RandomService = r_s.Next(1, 100);
            int used_index = (next_customer.AssignedServer.ID) - 1;
            int timeCasesCount = Servers[used_index].TimeDistribution.Count;

            for (int i = 0; i < timeCasesCount; i++)
            {

                if (next_customer.RandomService >= Servers[used_index].TimeDistribution[i].MinRange &&
                    next_customer.RandomService <= Servers[used_index].TimeDistribution[i].MaxRange)
                {

                    next_customer.ServiceTime = Servers[used_index].TimeDistribution[i].Time;
                    break;
                }
            }
            next_customer.EndTime = next_customer.StartTime + next_customer.ServiceTime;
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
                {
                    next_customer.AssignedServer.ID = Nearset_will_be_available[0];
                    calculate_service_time(ref next_customer);
                    Servers[next_customer.AssignedServer.ID - 1].TotalWorkingTime += next_customer.ServiceTime;
                }

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
            //@@check first if server is free??
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
        */
    }
}
