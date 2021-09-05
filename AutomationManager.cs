using Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for automated work.
    /// </summary>
    public class AutomationManager : IAutomationManager, IDisposable
	{
		private Thread automationWorker;
        private AutoResetEvent automationTrigger;
        private IStorage storage;
		private IProcessingManager processingManager;
		private int delayBetweenCommands;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationManager"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="automationTrigger">The automation trigger.</param>
        /// <param name="configuration">The configuration.</param>
        public AutomationManager(IStorage storage, IProcessingManager processingManager, AutoResetEvent automationTrigger, IConfiguration configuration)
		{
			this.storage = storage;
			this.processingManager = processingManager;
            this.configuration = configuration;
            this.automationTrigger = automationTrigger;
        }

        /// <summary>
        /// Initializes and starts the threads.
        /// </summary>
		private void InitializeAndStartThreads()
		{
			InitializeAutomationWorkerThread();
			StartAutomationWorkerThread();
		}

        /// <summary>
        /// Initializes the automation worker thread.
        /// </summary>
		private void InitializeAutomationWorkerThread()
		{
			automationWorker = new Thread(AutomationWorker_DoWork);
			automationWorker.Name = "Aumation Thread";
		}

        /// <summary>
        /// Starts the automation worker thread.
        /// </summary>
		private void StartAutomationWorkerThread()
		{
			automationWorker.Start();
		}


		private void AutomationWorker_DoWork()
		{
			EGUConverter eguConverter = new EGUConverter();
            while (!disposedValue)
            {
				PointIdentifier analogOut = new PointIdentifier(PointType.ANALOG_OUTPUT, 1000);
				PointIdentifier izlaz1 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2000);
				PointIdentifier izlaz2 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2001);
				PointIdentifier izlaz3 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2002);
				PointIdentifier izlaz4 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 2003);
				PointIdentifier punjac1 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 3000);
				PointIdentifier punjac2 = new PointIdentifier(PointType.DIGITAL_OUTPUT, 3001);
				List<PointIdentifier> points = new List<PointIdentifier>() { analogOut, izlaz1, izlaz2, izlaz3, izlaz4, punjac1, punjac2 };
				//
				List<IPoint> pointList = storage.GetPoints(points);
				//
				ushort value = pointList[0].RawValue;
				bool ugasiIzlaz = false;
				bool ugasiPunjac = false;


				// kad rezervoar stigne do manje od 1000 onda ugasiti izlaze i upaliti punjace
				if (value < eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 1000))
                {
					ugasiIzlaz = true;
                }

				// kad rezervoar stigne do vise od 8000 onda ugasiti punjace
				if(value > eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 8000))
                {
					ugasiPunjac = true;
                }
                // logika za rad izlaza
                if (!ugasiIzlaz)
				{
					if (pointList[1].RawValue == 1)
					{
						value -= eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 10);

					}
					if (pointList[2].RawValue == 1)
					{
						value -= eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 10);
					}
					if (pointList[3].RawValue == 1)
					{
						value -= eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 10);
					}
					if (pointList[4].RawValue == 1)
					{
						value -= eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 10);
					}

				}

                if (!ugasiPunjac)
                {
					if(pointList[5].RawValue == 1)
                    {
						value += eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 100);
					}
					if (pointList[6].RawValue == 1)
					{
						value += eguConverter.ConvertToRaw(pointList[0].ConfigItem.ScaleFactor, pointList[0].ConfigItem.Deviation, 50);
					}

					// pokusaj zabrane promene
					if (pointList[1].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[1].Address, 0);
					if (pointList[2].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[2].Address, 0);
					if (pointList[3].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[3].Address, 0);
					if (pointList[4].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[4].Address, 0);
				}

				// slanje updatovane vrednosti nazad u rezervoar
				if(value != pointList[0].RawValue)
                {
					processingManager.ExecuteWriteCommand(pointList[0].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[0].Address, value);
                }

                if (ugasiIzlaz)
                {
					// ugasi sve izlaze
					if(pointList[1].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[1].Address, 0);
					if(pointList[2].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[2].Address, 0);
					if (pointList[3].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[3].Address, 0);
					if (pointList[4].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[4].Address, 0);

					// upali sve punjace
					if(pointList[5].RawValue == 0)
						processingManager.ExecuteWriteCommand(pointList[5].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[5].Address, 1);
					if (pointList[6].RawValue == 0)
						processingManager.ExecuteWriteCommand(pointList[6].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[6].Address, 1);

				}

				// ugasi sve punjace
				if (ugasiPunjac)
				{
					if(pointList[5].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[5].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[5].Address, 0);
					if (pointList[6].RawValue == 1)
						processingManager.ExecuteWriteCommand(pointList[6].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[6].Address, 0);

				}

				automationTrigger.WaitOne(delayBetweenCommands);
			}
        }

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls


        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Indication if managed objects should be disposed.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				disposedValue = true;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// GC.SuppressFinalize(this);
		}

        /// <inheritdoc />
        public void Start(int delayBetweenCommands)
		{
			this.delayBetweenCommands = delayBetweenCommands*1000;
            InitializeAndStartThreads();
		}

        /// <inheritdoc />
        public void Stop()
		{
			Dispose();
		}
		#endregion
	}
}
