// Added by Pir Ubaid (USER STORY 204 : NPM ALERT )
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using NPMAPI.Models;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public partial class AlertService : IAlertRepository
    {
        public ResponseModel GetAlertForPatient(long patientaccount)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {

                using (var ctx = new NPMDBEntities())
                {
                    var useralert = ctx.USPGetAlertAgainst_PAT(patientaccount).ToList();
                    if (useralert != null && useralert.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = useralert;
                    }
                    else
                    {
                        responseModel.Status = "No data found";
                    }
                }


            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }
            return responseModel;
        }

        //alert enchancemnet
        public ResponseModel GetLoginAlert()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    // LINQ query: Select where Type = 'N', ApplicableFor = 'A', and Inactive is either null or false (active)
                    var loginalert = ctx.NpmAlerts
                                        .Where(alert => alert.Type == "N" &&
                                                        alert.ApplicableFor == "A" &&
                                                        (!alert.Inactive.HasValue || alert.Inactive == false))
                                        .ToList();

                    if (loginalert != null && loginalert.Any())
                    {
                        responseModel.Status = "Success";
                        responseModel.Response = loginalert;
                    }
                    else
                    {
                        responseModel.Status = "No data found";
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Status = ex.ToString();
            }

            return responseModel;
        }



        //--
        public ResponseModel SaveLoginAlert(NpmAlert model, long userId, string userName)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model != null)

                    {
                        NpmAlert Alert = ctx.NpmAlerts.Where(x => x.AlertID == model.AlertID).FirstOrDefault();
                        if (Alert != null)
                        {
                            Alert.AlertID = model.AlertID;
                            Alert.Type = model.Type;
                            Alert.ApplicableFor = model.ApplicableFor;
                            Alert.Priority = model.Priority;
                            Alert.EffectiveFrom = model.EffectiveFrom;
                            Alert.EffectiveTo = model.EffectiveTo;
                            Alert.ApplicableFor = model.ApplicableFor;
                            Alert.Demographics = model.Demographics;
                            Alert.AlertMessage = model.AlertMessage;
                         //   Alert.AddNewClaim = model.AddNewClaim;
                         
                            Alert.Modified_Date = DateTime.Now;
                            Alert.Modified_By = userId;
                            Alert.Created_User = userName;
                         
                            Alert.Inactive = model.Inactive;
                            ctx.Entry(Alert).State = EntityState.Modified;
                        }

                        else
                        {

                            ctx.NpmAlerts.Add(new NpmAlert()
                            {
                                AlertID = Convert.ToInt64(ctx.SP_TableIdGenerator("AlertID").FirstOrDefault().ToString()),
                                Type = model.Type,
                                EffectiveFrom = model.EffectiveFrom,
                                Priority = model.Priority,
                                EffectiveTo = model.EffectiveTo,
                                ApplicableFor = model.ApplicableFor,
                              
                                AlertMessage = model.AlertMessage,
                              
                               
                                Created_By = userId,
                                Created_User = userName,
                            
                                Inactive = model.Inactive,
                            });
                        }
                        ctx.SaveChanges();
                        objResponse.Status = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }

        public ResponseModel SaveAlert(NpmAlert model, long userId, string userName)
        {
            ResponseModel objResponse = new ResponseModel();
            try
            {
                using (var ctx = new NPMDBEntities())
                {
                    if (model != null)

                    {
                        NpmAlert Alert = ctx.NpmAlerts.Where(x => x.AlertID == model.AlertID).FirstOrDefault();
                        if (Alert != null)
                        {
                            Alert.AlertID = model.AlertID;
                            Alert.Type = model.Type;
                            Alert.ApplicableFor = model.ApplicableFor;
                            Alert.Priority = model.Priority;
                            Alert.EffectiveFrom = model.EffectiveFrom;
                            Alert.EffectiveTo = model.EffectiveTo;
                            Alert.ApplicableFor = model.ApplicableFor;
                            Alert.Demographics = model.Demographics;
                            Alert.AlertMessage = model.AlertMessage;
                            Alert.AddNewClaim = model.AddNewClaim;
                            Alert.Claim = model.Claim;
                            Alert.AddNewPayment = model.AddNewPayment;
                            Alert.ClaimSummary = model.ClaimSummary;
                            Alert.ClaimText = model.ClaimText;
                            Alert.Modified_Date = DateTime.Now;
                            Alert.Modified_By = userId;
                            Alert.Created_User = userName;
                            Alert.Patient_Account = model.Patient_Account;
                            Alert.Inactive = model.Inactive;
                            ctx.Entry(Alert).State = EntityState.Modified;
                        }

                        else
                        {

                            ctx.NpmAlerts.Add(new NpmAlert()
                            {
                                AlertID = Convert.ToInt64(ctx.SP_TableIdGenerator("AlertID").FirstOrDefault().ToString()),
                                Type = model.Type,
                                EffectiveFrom = model.EffectiveFrom,
                                Priority = model.Priority,
                                EffectiveTo = model.EffectiveTo,
                                ApplicableFor = model.ApplicableFor,
                                Demographics = model.Demographics,
                                AlertMessage = model.AlertMessage,
                                AddNewClaim = model.AddNewClaim,
                                Claim = model.Claim,
                                AddNewPayment = model.AddNewPayment,
                                ClaimSummary = model.ClaimSummary,
                                ClaimText = model.ClaimText,
                                Created_Date = DateTime.Now,
                                Created_By = userId,
                                Created_User = userName,
                                Patient_Account = model.Patient_Account,
                                Inactive = model.Inactive,
                            });
                        }
                        ctx.SaveChanges();
                        objResponse.Status = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                objResponse.Status = ex.ToString();
            }
            return objResponse;
        }
    }

}
    //


    //

