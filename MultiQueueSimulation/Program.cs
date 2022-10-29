﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiQueueTesting;
using MultiQueueModels;

namespace MultiQueueSimulation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string one = "D:\\Study\\Modelling & Simulation\\Tasks\\Task 1\\MultiQueueSimulation" +
                "\\MultiQueueSimulation\\TestCases\\TestCase3.txt";
            SimulationSystem system = new SimulationSystem();
            system = system.ReadFile(one);
            system.StartSimulation();
            string result = TestingManager.Test(system, Constants.FileNames.TestCase3);
            MessageBox.Show(result);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
           
        }
    }
}
