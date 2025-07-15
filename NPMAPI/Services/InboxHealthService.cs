using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NPMAPI.Models;
using NPMAPI.Repositories;
using Org.BouncyCastle.Asn1.Ocsp;

namespace NPMAPI.Services
{
    public class InboxHealthService : IPatientBilling
    {
        public int onPatientPaymentProcessed(dynamic eventData)
        {
            BATCHPAYMENT objModel = null;

            using (var ctx = new NPMDBEntities())
            {
                try
                {
                    PatientPaymentProcessed patientPaymentProcessed = new PatientPaymentProcessed
                    {
                        PatientPaymentProcessed_id = eventData.@object.id,
                        created_at = eventData.@object.created_at,
                        updated_at = eventData.@object.updated_at,
                        date_scheduled = eventData.@object.date_scheduled,
                        patient_payment_info_id = eventData.@object.patient_payment_info_id,
                        payment_plan_id = eventData.@object.payment_plan_id,
                        status = eventData.@object.status,
                        submitted_at = eventData.@object.submitted_at,
                        transfer_id = eventData.@object.transfer_id,
                        date_cancelled = eventData.@object.date_cancelled,
                        card_brand = eventData.@object.card_brand,
                        bank_number = eventData.@object.bank_number,
                        payment_method_type = eventData.@object.payment_method_type,
                        description = eventData.@object.description,
                        batch_number = eventData.@object.batch_number,
                        secondary_patient_payment_info_id = eventData.@object.secondary_patient_payment_info_id,
                        created_by_user_id = eventData.@object.created_by_user_id,
                        total_fee = eventData.@object.total_fee_cents,
                        refunded_amount = eventData.@object.refunded_amount_cents,
                        adjusted_fee = eventData.@object.adjusted_fee_cents,
                        enterprise_id = eventData.@object.enterprise_id,
                        patient_id = eventData.@object.patient_id,
                        transferred_at = eventData.@object.transferred_at,
                        applied_amount = eventData.@object.applied_amount_cents,
                        practice_id = eventData.@object.practice_id,
                        doctor_id = eventData.@object.doctor_id,
                        voided = eventData.@object.voided,
                        signed = eventData.@object.signed,
                        dispute_status = eventData.@object.dispute_status,
                        dispute_id = eventData.@object.dispute_id,
                        reversal_amount = eventData.@object.reversal_amount_cents,
                        payer_description = eventData.@object.payer_description,
                        merged_payment_id = eventData.@object.merged_payment_id,
                        deposit_account_id = eventData.@object.deposit_account_id,
                        sub_type = eventData.@object.sub_type,
                        is_posting = eventData.@object.is_posting,
                        last_4_digits = eventData.@object.last_4_digits,
                        has_attachments = eventData.@object.has_attachments,
                        can_change_submitted_at_date = eventData.@object.can_change_submitted_at_date,
                        is_archive_pending = eventData.@object.is_archive_pending
                    };
                    ctx.PatientPaymentProcesseds.Add(patientPaymentProcessed);
                    ctx.SaveChanges();



                    long synncedPractice = Convert.ToInt64( eventData.@object.enterprise_id);
                    string SynncedPractice = ctx.SyncedPractices.SingleOrDefault(p => p.GeneratedId == synncedPractice ).Practice_Code.ToString(); ;
                    long synncedpat =Convert.ToInt64(eventData.@object.patient_id) ;
                    long patAccount = ctx.SyncedPatients.SingleOrDefault(p => p.GeneratedId == synncedpat).Patient_Account;
                    string firstName = ctx.Patients.SingleOrDefault(p => p.Patient_Account == patAccount).First_Name;
                    string lastName = ctx.Patients.SingleOrDefault(p => p.Patient_Account == patAccount).Last_Name;


                   


                    objModel = new BATCHPAYMENT();
                    long BatchNo = Convert.ToInt64(ctx.SP_TableIdGenerator("BatchNo").FirstOrDefault().ToString());
                    objModel.BatchNo = BatchNo;
                    objModel.PatientName = firstName +" "+ lastName;
                    objModel.PaymentTypeID = 3;
                    objModel.FacilityID = null;
                    objModel.PostedAmount = 0;
                    objModel.DepositDate = eventData.@object.submitted_at;
                    objModel.Amount =( eventData.@object.applied_amount_cents / 100) ;
                    objModel.PatientAccount = patAccount;
                    objModel.practice_code = SynncedPractice;
                    objModel.CheckDate = null;
                    objModel.CheckNo = null;
                    objModel.NOtes = null;
                    objModel.EOBDate = null;
                    objModel.ReceivedDate = DateTime.Now;
                    objModel.InsuranceID = null;

                    ctx.BATCHPAYMENTS.Add(objModel);
                   return ctx.SaveChanges();



                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}