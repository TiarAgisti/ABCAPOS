using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Transactions;
using ABCAPOS.Util;

namespace ABCAPOS.BF
{
    public class AllowanceBFC : MasterDetailBFC<Allowance, Allowance, AllowanceDetail, v_AllowanceDetail, AllowanceModel, AllowanceDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetAllowanceCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var allowancePrefix = "";

            if (prefixSetting != null)
                allowancePrefix = prefixSetting.AllowancePrefix;

            var code = new ABCAPOSDAC().RetrieveAllowanceMaxCode(allowancePrefix, 5);

            return code;
        }

        protected override GenericDetailDAC<AllowanceDetail, AllowanceDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<AllowanceDetail, AllowanceDetailModel>("AllowanceID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_AllowanceDetail, AllowanceDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_AllowanceDetail, AllowanceDetailModel>("AllowanceID", "ItemNo", false);
        }

        protected override GenericDAC<Allowance, AllowanceModel> GetMasterDAC()
        {
            return new GenericDAC<Allowance, AllowanceModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<Allowance, AllowanceModel> GetMasterViewDAC()
        {
            return new GenericDAC<Allowance, AllowanceModel>("ID", false, "Code DESC");
        }

        public override void Create(AllowanceModel header, List<AllowanceDetailModel> details)
        {
            header.Code = GetAllowanceCode();

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                OnUpdated(header.ID);

                trans.Complete();
            }
        }

        public override void Update(AllowanceModel header, List<AllowanceDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                NormalizeOldStaffData(header.ID);

                base.Update(header, details);

                OnUpdated(header.ID);

                trans.Complete();
            }
        }

        public void CreateFirstAllowance(AllowanceModel header)
        {
            var staffList = new StaffBFC().Retrieve(true);

            var details = new List<AllowanceDetailModel>();

            foreach (var staff in staffList)
            {
                var detail = new AllowanceDetailModel();

                detail.StaffID = staff.ID;
                detail.StaffName = staff.Name;
                detail.isPaidLoan = true;

                details.Add(detail);
            }

            header.Details = details;
        }

        public void Validate(AllowanceModel header, List<AllowanceDetailModel> details)
        {
            foreach (var detail in details)
            {
                var staff = new StaffBFC().RetrieveByID(detail.StaffID);

                detail.BasicSalary = staff.BasicSalary;
                detail.TransportAllowance = staff.TransportAllowance;
                detail.ActiveBonus = staff.ActiveBonus;
                detail.PositionAllowance = staff.PositionAllowance;
                detail.MealAllowance = staff.MealAllowance;
                detail.Bonus = staff.Bonus;

                if (header.isTHRPaid == true)
                    detail.THR = staff.THR;
                else
                    detail.THR = 0;

                detail.MealAllowanceExpense = staff.MealAllowanceExpense;

                if (staff.LoanAmount > 0)
                {
                    if (header.ID != 0)
                    {
                        var oldDetails = RetrieveDetails(header.ID);

                        var oldDetail = (from i in oldDetails
                                         where i.StaffID == detail.StaffID
                                         select i).FirstOrDefault();

                        if (oldDetail != null)
                        {
                            staff.LoanAmount += oldDetail.PaidLoanAmount;
                            staff.InstallmentCount += 1;
                        }
                    }

                    if (detail.isPaidLoan == true)
                    {
                        detail.PaidLoanAmount = staff.LoanAmount / staff.InstallmentCount;
                    }
                    else
                    {
                        detail.PaidLoanAmount = 0;
                    }
                    
                    detail.LoanAmount = staff.LoanAmount - detail.PaidLoanAmount;
                    //detail.InstallmentOrderNo = staff.LastInstallmentNo + 1;
                }
            }
        }

        private void NormalizeOldStaffData(long allowanceID)
        {
            var details = RetrieveDetails(allowanceID);

            foreach (var detail in details)
            {
                var staff = new StaffBFC().RetrieveByID(detail.StaffID);

                if (detail.PaidLoanAmount > 0 && staff.LastInstallmentNo > 0 && detail.isPaidLoan == true)
                {
                    staff.LastInstallmentNo -= 1;
                    staff.InstallmentCount += 1;
                    staff.LoanAmount += detail.PaidLoanAmount;

                    new StaffBFC().Update(staff);
                }
            }
        }

        public void PostAccounting(long allowanceID)
        {
            var account = new AccountBFC().RetrieveByUserCode(SystemConstants.SalaryAccountUserCode);

            var results = new List<AccountingResultModel>();

            var allowance = RetrieveByID(allowanceID);
            var allowanceDetails = RetrieveDetails(allowanceID);

            var amount = allowanceDetails.Sum(p => p.IncomeAmount);

            var result = new AccountingResultModel();

            result.DocumentID = allowance.ID;
            result.DocumentType = (int)AccountingResultDocumentType.Allowance;
            result.Type = (int)AccountingResultType.Debit;
            result.Date = allowance.Date;
            result.AccountID = account.ID;
            result.DocumentNo = allowance.Code;
            result.Amount = amount;
            result.DebitAmount = amount;
            result.Remarks = "Slip Gaji no " + allowance.Code;
            results.Add(result);

            result = new AccountingResultModel();
            result.DocumentID = allowance.ID;
            result.DocumentType = (int)AccountingResultDocumentType.Allowance;
            result.Type = (int)AccountingResultType.Credit;
            result.Date = allowance.Date;
            //result.AccountID = account.ReferenceID;
            result.DocumentNo = allowance.Code;
            result.Amount = amount;
            result.CreditAmount = amount;
            result.Remarks = "Slip Gaji no " + allowance.Code;

            results.Add(result);

            new AccountingResultBFC().Posting(results);
        }

        private void OnUpdated(long allowanceID)
        {
            var allowanceDetails = RetrieveDetails(allowanceID);

            foreach (var allowanceDetail in allowanceDetails)
            {
                if (allowanceDetail.PaidLoanAmount > 0 && allowanceDetail.isPaidLoan == true)
                {
                    var staff = new StaffBFC().RetrieveByID(allowanceDetail.StaffID);

                    allowanceDetail.InstallmentOrderNo = staff.LastInstallmentNo + 1;
                    staff.LastInstallmentNo = allowanceDetail.InstallmentOrderNo;
                    staff.InstallmentCount -= 1;
                    staff.LoanAmount -= allowanceDetail.PaidLoanAmount;
                    allowanceDetail.InstallmentOrderNo = staff.InstallmentCount;

                    new StaffBFC().Update(staff);
                }
            }

            GetDetailDAC().DeleteByParentID(allowanceID);
            GetDetailDAC().Create(allowanceID, allowanceDetails);

            new AccountingResultBFC().Void(allowanceID, AccountingResultDocumentType.Allowance);
            PostAccounting(allowanceID);
        }
    }
}
