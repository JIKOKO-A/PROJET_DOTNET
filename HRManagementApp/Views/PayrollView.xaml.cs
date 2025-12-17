using System.Windows.Controls;
using HRManagementApp.ViewModels;
using System.Windows;
using System;

namespace HRManagementApp.Views;

public partial class PayrollView : UserControl
{
    public PayrollView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is PayrollViewModel viewModel)
        {
            viewModel.EditPayrollCommand.Execute(null);
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is PayrollViewModel viewModel && viewModel.SelectedPayroll != null)
        {
            // Auto-recalculate net salary when values change
            try
            {
                var payroll = viewModel.SelectedPayroll;
                if (decimal.TryParse(payroll.BaseSalary.ToString(), out decimal baseSalary) &&
                    decimal.TryParse(payroll.Deductions.ToString(), out decimal deductions) &&
                    decimal.TryParse(payroll.Bonuses.ToString(), out decimal bonuses))
                {
                    payroll.NetSalary = baseSalary - deductions + bonuses;
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }
    }
}


