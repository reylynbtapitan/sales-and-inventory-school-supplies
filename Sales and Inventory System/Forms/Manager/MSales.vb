Imports System.Windows.Forms.DataVisualization.Charting
Public Class MSales

    Private Sub MSales_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        Guna2HtmlLabel19.Text = CurrentUser

        ' Load transactions and sales summary
        LoadTransactions()
        LoadSalesSummary()
        LoadTransactionChart()
    End Sub
    Private Sub LoadTransactions(Optional ByVal filterDate As Date? = Nothing, Optional ByVal searchText As String = "")
        Try
            Dim sql As String = "SELECT TransactionID, DateAndTime, Staff, Gross, Vat, Total FROM Transactions WHERE 1=1"
            Parameters.Clear()

            ' Filter by date if specified
            If filterDate.HasValue Then
                sql &= " AND CONVERT(date, DateAndTime) = @dateFilter"
                AddParam("@dateFilter", filterDate.Value.Date)
            End If

            ' Search by Staff or TransactionID
            If Not String.IsNullOrWhiteSpace(searchText) Then
                sql &= " AND (Staff LIKE @search OR CAST(TransactionID AS NVARCHAR) LIKE @search)"
                AddParam("@search", "%" & searchText.Trim() & "%")
            End If

            sql &= " ORDER BY DateAndTime DESC"

            Dim dt As DataTable = ExecuteQuery(sql)

            DgvTransactions.DataSource = Nothing
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                DgvTransactions.DataSource = dt

                ' Format columns
                With DgvTransactions
                    If .Columns.Contains("TransactionID") Then .Columns("TransactionID").HeaderText = "ID"
                    If .Columns.Contains("DateAndTime") Then .Columns("DateAndTime").HeaderText = "Date & Time"
                    If .Columns.Contains("Staff") Then .Columns("Staff").HeaderText = "Staff"
                    If .Columns.Contains("Gross") Then .Columns("Gross").DefaultCellStyle.Format = "N2"
                    If .Columns.Contains("Vat") Then .Columns("Vat").DefaultCellStyle.Format = "N2"
                    If .Columns.Contains("Total") Then .Columns("Total").DefaultCellStyle.Format = "N2"

                    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                    .SelectionMode = DataGridViewSelectionMode.FullRowSelect
                    .ReadOnly = True
                End With
            End If

        Catch ex As Exception
            MessageBox.Show("Failed to load transactions: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtSearch.TextChanged
        LoadTransactions(DateTimePicker1.Value, TxtSearch.Text)
    End Sub
    Private Sub DtpFilter_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DateTimePicker1.ValueChanged
        LoadTransactions(DateTimePicker1.Value, TxtSearch.Text)
    End Sub
    Private Sub LoadSalesSummary()
        Try
            Dim sql As String = "SELECT " &
                                "ISNULL(SUM(CASE WHEN CONVERT(date, DateAndTime) = @today THEN Total END),0) AS TodaySales," &
                                "ISNULL(SUM(CASE WHEN DATEPART(week, DateAndTime) = DATEPART(week, @today) AND YEAR(DateAndTime) = YEAR(@today) THEN Total END),0) AS WeeklySales," &
                                "ISNULL(SUM(CASE WHEN MONTH(DateAndTime) = MONTH(@today) AND YEAR(DateAndTime) = YEAR(@today) THEN Total END),0) AS MonthlySales," &
                                "ISNULL(SUM(CASE WHEN YEAR(DateAndTime) = YEAR(@today) THEN Total END),0) AS YearlySales," &
                                "ISNULL(SUM(Total),0) AS AccumulatedSales," &
                                "ISNULL(SUM(Vat),0) AS AccumulatedVAT " &
                                "FROM Transactions"

            Parameters.Clear()
            AddParam("@today", DateTime.Now.Date)

            Dim dt As DataTable = ExecuteQuery(sql)
            If dt.Rows.Count > 0 Then
                Dim row = dt.Rows(0)
                LblTodaySales.Text = Convert.ToDecimal(row("TodaySales")).ToString("N2")
                LblWeeklySales.Text = Convert.ToDecimal(row("WeeklySales")).ToString("N2")
                LblMonthlySales.Text = Convert.ToDecimal(row("MonthlySales")).ToString("N2")
                LblYearlySales.Text = Convert.ToDecimal(row("YearlySales")).ToString("N2")
                LblAccumulatedSales.Text = Convert.ToDecimal(row("AccumulatedSales")).ToString("N2")
                LblAccumulatedVAT.Text = Convert.ToDecimal(row("AccumulatedVAT")).ToString("N2")
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading sales summary: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
   
    Private Sub LoadTransactionChart()
        Try
            Chart1.Series.Clear()
            Dim series As New Series("Daily Sales") With {
                .ChartType = SeriesChartType.Line,
                .XValueType = ChartValueType.Date,
                .BorderWidth = 3,
                .MarkerStyle = MarkerStyle.Circle,
                .MarkerSize = 7
            }

            Dim sql As String = "SELECT CONVERT(date, DateAndTime) AS TranDate, SUM(Gross) AS TotalGross " &
                                "FROM Transactions " &
                                "GROUP BY CONVERT(date, DateAndTime) " &
                                "ORDER BY TranDate"

            Dim dt As DataTable = ExecuteQuery(sql)
            For Each row As DataRow In dt.Rows
                series.Points.AddXY(Convert.ToDateTime(row("TranDate")), Convert.ToDecimal(row("TotalGross")))
            Next

            Chart1.Series.Add(series)
            With Chart1.ChartAreas(0)
                .AxisX.Title = "Date"
                .AxisY.Title = "Gross Sales"
                .AxisX.LabelStyle.Format = "MM/dd"
                .AxisX.Interval = 1
                .AxisX.IntervalType = DateTimeIntervalType.Days
            End With

        Catch ex As Exception
            MessageBox.Show("Failed to load chart data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton6.Click
        ManagerDashboard.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    
    End Sub

    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton7.Click
        MAccounts.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
      
    End Sub

    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub

    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton3.Click

    End Sub

    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton4.Click
        MInventory.Show()
        Me.Hide()
    End Sub
End Class