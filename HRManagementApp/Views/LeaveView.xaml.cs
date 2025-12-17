using System.Windows.Controls;
using HRManagementApp.ViewModels;

namespace HRManagementApp.Views;

public partial class LeaveView : UserControl
{
    public LeaveView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is LeaveViewModel viewModel)
        {
            viewModel.EditLeaveRequestCommand.Execute(null);
        }
    }
}


