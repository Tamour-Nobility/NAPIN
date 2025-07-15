using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NPMSyncWorker.Entities;
using NPMSyncWorker.Models;
using NPMSyncWorker.Models.Request;
using NPMSyncWorker.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPMSyncWorker
{
    internal class Worker : BackgroundService
    {
        private readonly int _delay;
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitofWork _unitofWork;
        private readonly IExternalSynchronization _externalSynchronization;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IUnitofWork unitofWork,
            IExternalSynchronization externalSynchronization)
        {
            _logger = logger;
            _unitofWork = unitofWork;
            _configuration = configuration;
            _delay = _configuration.GetValue<int>("delay");
            _externalSynchronization = externalSynchronization;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                StartWorking();
                await Task.Delay(_delay, stoppingToken);
            }
        }

        private void StartWorking()
        {
            try
            {
                var practices = _unitofWork.PracticeRepository.GetPracticesForSync();
                if (practices?.Count > 0)
                {
                    foreach (var practice in practices)
                    {
                        //var practiceLocations = _unitofWork.PracticeLocationsRepository.GetPracticeLocationsForSync(practice.Practice_Code);
                        //object o = new
                        //{
                        //    practice = practice,
                        //    locations = practiceLocations
                        //};
                        BackgroundWorker syncWorker = new BackgroundWorker();
                        syncWorker.DoWork += StartPracticeSynchronization;
                        syncWorker.RunWorkerAsync(argument: practice);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void StartPracticeSynchronization(object sender, DoWorkEventArgs e)
        {
            PracticeToSync practice = (PracticeToSync)e.Argument;
            
            #region Enterprise
            EnterpriseCreateRequest enterprise = new EnterpriseCreateRequest
            {
                enterprise = new Enterprise()
                {
                    name = practice.Prac_Name,
                    city = practice.Prac_City,
                    state = practice.Prac_State,
                    address_line_1 = practice.Prac_Address,
                    address_line_2 = practice.Prac_Address_Line2,
                    zip = practice.Prac_Zip,
                    support_phone_number = practice.Prac_Phone,
                    sales_tax = Convert.ToInt32(practice.Prac_Tax_Id),
                    default_quick_pay_description = "Copay",
                    statement_descriptor = "Nobility RCM",
                    time_zone = "Eastern Time (US & Canada)",
                    color_statements = true,
                    first_class = true,
                    return_envelope = true,
                    perforation = true,
                    minimum_bill_amount_cents = 50
                }
            };


            var enterpriceCreateResponse = await _externalSynchronization.AddPractice(enterprise);
            // Failure to post Practice/ Enterprise
            // Return
            if (!enterpriceCreateResponse.IsSuccessful)
            {
                _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                {
                    LogMessage = enterpriceCreateResponse.ErrorMessage,
                    PracticeSyncId = practice.PracticeSyncId
                });

                _unitofWork.SynchronizationRepository.Update(new PracticeSynchronization()
                {
                    PracticeSyncId = practice.PracticeSyncId,
                    IsFailed = true,
                    Notes = "Synchronization failed.",
                });
                _unitofWork.SaveChanges();
                return;
            }
            else
            {
                // Success to post Practice/ Enterprise
                _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                {
                    LogMessage = "Practice with " + practice.Practice_Code + " has been successfuly synchronized",
                    PracticeSyncId = practice.PracticeSyncId
                });
                _unitofWork.SaveChanges();
            }
            #endregion

            #region Practice Posting
            if (enterpriceCreateResponse.IsSuccessful)
            {
                var practiceLocations = _unitofWork.PracticeLocationsRepository.GetPracticeLocationsForSync(practice.Practice_Code);
                if (practiceLocations is not null && practiceLocations.Count > 0)
                {
                    foreach (var location in practiceLocations)
                    {
                        var practiceCreateResponse = await _externalSynchronization.AddPracticeAttribute(new PracticeCreateRequest()
                        {
                            practicesAttribute = new PracticesAttribute()
                            {

                            }
                        });
                        // Failure to post PracticeLocation
                        // Return
                        if (!practiceCreateResponse.IsSuccessful)
                        {
                            _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                            {
                                LogMessage = practiceCreateResponse.ErrorMessage,
                                PracticeSyncId = practice.PracticeSyncId
                            });

                            _unitofWork.SynchronizationRepository.Update(new PracticeSynchronization()
                            {
                                PracticeSyncId = practice.PracticeSyncId,
                                IsFailed = true,
                                Notes = "Synchronization failed.",
                            });
                            _unitofWork.SaveChanges();
                            return;
                        }
                        else
                        {
                            // Success to post Practice/ Enterprise
                            _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                            {
                                LogMessage = "Practice with " + practice.Practice_Code + " has been successfuly synchronized",
                                PracticeSyncId = practice.PracticeSyncId
                            });
                            _unitofWork.SaveChanges();
                        }
                    }
                }
            }
            #endregion


            // Continue posting of other entities

            #region Patients Posting
            if (enterpriceCreateResponse.IsSuccessful)
            {
                var patients = _unitofWork.PatientRepository.GetPatientsForSync(practice.Practice_Code);
                if (patients is not null && patients.Count > 0)
                {
                    foreach (var patient in patients)
                    {
                        var patientCreateResponse = await _externalSynchronization.AddPatient(new PatientCreateRequest()
                        {
                            patient = new Models.Patient()
                            {
                                first_name = patient.First_Name,
                                last_name = patient.Last_Name,
                                date_of_birth = patient.Date_Of_Birth.ToString("yyyyy-MM-dd"),
                                primary = true,
                                sex = patient.Gender.ToString(),
                                enterprise_id = enterpriceCreateResponse.id
                            }
                        });
                        if (!patientCreateResponse.IsSuccessful)
                        {
                            _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                            {
                                LogMessage = enterpriceCreateResponse.ErrorMessage + " Patient with " + patient.Patient_Account + " has been failed for synchronization",
                                PracticeSyncId = practice.PracticeSyncId
                            });
                            _unitofWork.SaveChanges();
                        }
                        else
                        {
                            _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                            {
                                LogMessage = " Patient with " + patient.Patient_Account + " has been successfuly synchronized",
                                PracticeSyncId = practice.PracticeSyncId
                            });
                            _unitofWork.SaveChanges();
                        }
                    }
                }
            }
            #endregion

            #region Providers Posting
            if (enterpriceCreateResponse.IsSuccessful)
            {
                var providers = _unitofWork.ProviderRepository.GetProvidersForSync(practice.Practice_Code);
                if (providers is not null && providers.Count > 0)
                {
                    foreach (var provider in providers)
                    {
                        var providerCreateResponse = await _externalSynchronization.AddProvider(new ProviderCreateRequest()
                        {
                            doctor = new Doctor()
                            {
                                first_name = provider.Provid_FName,
                                last_name = provider.Provid_LName,
                                middle_name = provider.Provid_Middle_Name,
                                // TODO: Provider mising fields
                                phone = "000-000-0000",
                                specialty = "",
                                email = provider.Email_Address,
                                certification = "",
                                suffix = "",
                                message = "",
                                npi = provider.NPI,
                                status = "floating",
                                provider_code = provider.Provider_Code,
                                deposit_account_id = null,
                                shifts_attributes = new List<ShiftsAttribute>()
                                {
                                    new ShiftsAttribute
                                    {
                                        practice_id=Convert.ToInt32(practice.Practice_Code),
                                        start_time=new DateTime().ToString("yyyy-MM-dd hh:mm:ss"),
                                        end_time=new DateTime().ToString("yyyy-MM-dd hh:mm:ss"),
                                        is_available=true
                                    }
                                },
                                enterprise_id = enterpriceCreateResponse.id
                            }
                        });
                        if (!providerCreateResponse.IsSuccessful)
                        {
                            _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                            {
                                LogMessage = enterpriceCreateResponse.ErrorMessage + " Provider with " + provider.Provider_Code + " has been failed for synchronization",
                                PracticeSyncId = practice.PracticeSyncId
                            });
                            _unitofWork.SaveChanges();
                        }
                        else
                        {
                            _unitofWork.SynchronizationLogRepository.Add(new PracticeSynchronizationLog()
                            {
                                LogMessage = " Provider with " + provider.Provider_Code + " has been successfuly synchronized",
                                PracticeSyncId = practice.PracticeSyncId
                            });
                            _unitofWork.SaveChanges();
                        }
                    }
                }
            }
            #endregion
        }

    }
}
