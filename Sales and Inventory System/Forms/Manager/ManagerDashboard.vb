Imports System.Windows.Forms.DataVisualization.Charting
Public Class ManagerDashboard

    Private Sub ManagerDashboard_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel19.Text = CurrentUser
        Guna2HtmlLabel2.Text = CurrentUser
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")

        LoadSalesChart(Chart1)
        Dim todaySales As Decimal = GetTodaySalesAmount()
        LblTodaySales.Text = "₱" & todaySales.ToString("N2")

        LoadTotalItemsToLabel(LblTotalItems)
        LoadLowAndOutOfStockToGrid(DGVLowStock, LblLowCount, LblOutCount)

        CountAllUsersByRole(LblTotalUsers, LblAdmin, LblCashier, LblStaff, LblManager)

        LoadApprovalRequests()
    End Sub
    Public Sub CountAllUsersByRole(
     ByVal lblTotalUsers As Object,
     ByVal lblAdmin As Object,
     ByVal lblCashier As Object,
     ByVal lblStaff As Object,
     ByVal lblManager As Object
 )
        Try
            Dim sql As String = "SELECT Role, COUNT(*) AS RoleCount FROM Users GROUP BY Role"
            Dim dt As DataTable = ExecuteQuery(sql)

            ' Reset labels
            lblTotalUsers.Text = 0
            lblAdmin.Text = 0
            lblCashier.Text = 0
            lblStaff.Text = 0
            lblManager.Text = 0

            ' Assign values based on roles
            For Each row As DataRow In dt.Rows
                Select Case row("Role").ToString()
                    Case "Admin"
                        lblAdmin.Text = row("RoleCount").ToString()
                    Case "Cashier"
                        lblCashier.Text = row("RoleCount").ToString()
                    Case "Staff"
                        lblStaff.Text = row("RoleCount").ToString()
                    Case "Manager"
                        lblManager.Text = row("RoleCount").ToString()
                End Select
            Next

            ' Total users
            lblTotalUsers.Text = dt.Compute("SUM(RoleCount)", "").ToString()

        Catch ex As Exception
            MessageBox.Show("Error counting users by role: " & ex.Message)
        End Try
    End Sub

    Public Sub LoadSalesChart(ByVal chartControl As Chart)
        Try
            chartControl.Series.Clear()

            Dim salesSeries As New Series("Daily Sales")
            salesSeries.ChartType = SeriesChartType.Line
            salesSeries.XValueType = ChartValueType.Date
            salesSeries.BorderWidth = 3
            salesSeries.MarkerStyle = MarkerStyle.Circle
            salesSeries.MarkerSize = 7

            Dim sql As String = "" &
                "SELECT CONVERT(date, DateAndTime) AS TranDate, SUM(Gross) AS TotalGross " &
                "FROM Transactions " &
                "GROUP BY CONVERT(date, DateAndTime) " &
                "ORDER BY TranDate"

            Dim dt As DataTable = ExecuteQuery(sql)

            For Each row As DataRow In dt.Rows
                salesSeries.Points.AddXY(Convert.ToDateTime(row("TranDate")), Convert.ToDecimal(row("TotalGross")))
            Next

            chartControl.Series.Add(salesSeries)

            With chartControl.ChartAreas(0)
                .AxisX.Title = "Date"
                .AxisY.Title = "Gross Sales"
                .AxisX.LabelStyle.Format = "MM/dd"
                .AxisX.Interval = 1
                .AxisX.IntervalType = DateTimeIntervalType.Days
            End With

        Catch ex As Exception
            MessageBox.Show("Failed to load chart data: " & ex.Message, "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Function GetTodaySalesAmount() As Decimal
        Try
            Dim sql As String = "" &
                "SELECT ISNULL(SUM(Gross), 0) AS TodayTotal " &
                "FROM Transactions " &
                "WHERE CONVERT(date, DateAndTime) = @today"

            Parameters.Clear()
            AddParam("@today", DateTime.Now.Date)

            Dim dt As DataTable = ExecuteQuery(sql)
            If dt.Rows.Count > 0 Then
                Return Convert.ToDecimal(dt.Rows(0)("TodayTotal"))
            Else
                Return 0D
            End If

        Catch ex As Exception
            MessageBox.Show("Error fetching today's sales: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return 0D
        End Try
    End Function
    Private Sub LoadApprovalRequests()
        Try
            ' ✅ Load all request types
            Dim sql As String = "SELECT RequestID, RequestType, RequestedBy, Details, Status, DateRequested, DateActioned " &
                                "FROM ApprovalRequests " &
                                "ORDER BY DateRequested DESC"

            ' Execute the SQL query
            Dim dt As DataTable = ExecuteQuery(sql)

            ' Bind data to DataGridView
            DGVApprovalRequests.DataSource = dt

            ' Format DataGridView columns (if data exists)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                With DGVApprovalRequests
                    .Columns("RequestID").HeaderText = "Request ID"
                    .Columns("RequestType").HeaderText = "Request Type"
                    .Columns("RequestedBy").HeaderText = "Requested By"
                    .Columns("Details").HeaderText = "Details"
                    .Columns("Status").HeaderText = "Status"
                    .Columns("DateRequested").HeaderText = "Date Requested"
                    .Columns("DateActioned").HeaderText = "Date Actioned"

                    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                    .SelectionMode = DataGridViewSelectionMode.FullRowSelect
                    .ReadOnly = True
                End With
            End If

        Catch ex As Exception
            MessageBox.Show("Failed to load approval requests: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        MInventory.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        MInventory.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        MAccounts.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        MAccounts.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Hide()
        MSales.Show()
    End Sub

    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        GlobalLogout()
    End Sub
End Class
