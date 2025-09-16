using System;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for calculating interest using different methods
    /// </summary>
    public class InterestCalculationService
    {
        /// <summary>
        /// Calculate interest using flat rate method
        /// </summary>
        /// <param name="principal">Principal amount</param>
        /// <param name="rate">Interest rate (as decimal, e.g., 0.10 for 10%)</param>
        /// <param name="timeInMonths">Time period in months</param>
        /// <returns>Total interest amount</returns>
        public decimal CalculateFlatInterest(decimal principal, decimal rate, int timeInMonths)
        {
            return principal * rate * (timeInMonths / 12m);
        }

        /// <summary>
        /// Calculate interest using reducing balance method
        /// </summary>
        /// <param name="principal">Principal amount</param>
        /// <param name="rate">Monthly interest rate (as decimal)</param>
        /// <param name="timeInMonths">Time period in months</param>
        /// <returns>Total interest amount</returns>
        public decimal CalculateReducingBalanceInterest(decimal principal, decimal rate, int timeInMonths)
        {
            decimal monthlyRate = rate / 12m;
            decimal totalInterest = 0;
            decimal remainingPrincipal = principal;

            for (int month = 1; month <= timeInMonths; month++)
            {
                decimal monthlyInterest = remainingPrincipal * monthlyRate;
                totalInterest += monthlyInterest;
                
                // Calculate monthly payment (principal + interest)
                decimal monthlyPayment = principal / timeInMonths + monthlyInterest;
                remainingPrincipal -= (monthlyPayment - monthlyInterest);
            }

            return totalInterest;
        }

        /// <summary>
        /// Calculate interest using compound interest method
        /// </summary>
        /// <param name="principal">Principal amount</param>
        /// <param name="rate">Annual interest rate (as decimal)</param>
        /// <param name="timeInMonths">Time period in months</param>
        /// <param name="compoundingFrequency">Number of times interest is compounded per year</param>
        /// <returns>Total interest amount</returns>
        public decimal CalculateCompoundInterest(decimal principal, decimal rate, int timeInMonths, int compoundingFrequency = 12)
        {
            decimal timeInYears = timeInMonths / 12m;
            decimal amount = principal * (decimal)Math.Pow((double)(1 + rate / compoundingFrequency), (double)(compoundingFrequency * timeInYears));
            return amount - principal;
        }

        /// <summary>
        /// Calculate monthly payment for a loan
        /// </summary>
        /// <param name="principal">Principal amount</param>
        /// <param name="rate">Monthly interest rate (as decimal)</param>
        /// <param name="timeInMonths">Time period in months</param>
        /// <returns>Monthly payment amount</returns>
        public decimal CalculateMonthlyPayment(decimal principal, decimal rate, int timeInMonths)
        {
            if (rate == 0)
                return principal / timeInMonths;

            decimal monthlyRate = rate / 12m;
            decimal power = (decimal)Math.Pow((double)(1 + monthlyRate), timeInMonths);
            return principal * (monthlyRate * power) / (power - 1);
        }

        /// <summary>
        /// Calculate penalty for overdue payments
        /// </summary>
        /// <param name="overdueAmount">Overdue amount</param>
        /// <param name="penaltyRate">Penalty rate per day (as decimal)</param>
        /// <param name="daysOverdue">Number of days overdue</param>
        /// <returns>Penalty amount</returns>
        public decimal CalculatePenalty(decimal overdueAmount, decimal penaltyRate, int daysOverdue)
        {
            return overdueAmount * penaltyRate * daysOverdue;
        }
    }
}