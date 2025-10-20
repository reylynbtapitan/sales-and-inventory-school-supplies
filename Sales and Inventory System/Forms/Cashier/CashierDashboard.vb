Imports System.Windows.Forms.DataVisualization.Charting

Public Class CashierDashboard

    Private Sub CashierDashboard_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel19.Text = CurrentUser

        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        LoadChartFromTransactions()

        Dim salesSummary As Dictionary(Of String, Decimal) = GetSalesSummary()
        LblTodaySales.Text = "₱" & salesSummary("Today").ToString("N2")

        LoadTodaysTransactions(DgvTransactions)


        Dim todayTransactionCount As Integer = GetDgvRowCount(DgvTransactions)
        Guna2HtmlLabel5.Text = "Number of transactions today: " & todayTransactionCount
    End Sub
    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub
    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton4.Click
        Sales.Show()
        Me.Hide()
    End Sub
    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton3.Click
        Me.Hide()
        CashierInventory.Show()
    End Sub
    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Hide()
        History.Show()
    End Sub
    Public Sub LoadChartFromTransactions()
        Try
            Chart1.Series.Clear()

            Dim salesSeries As New Series("Daily Sales")
            salesSeries.ChartType = SeriesChartType.Line
            salesSeries.XValueType = ChartValueType.Date
            salesSeries.BorderWidth = 3
            salesSeries.MarkerStyle = MarkerStyle.Circle
            salesSeries.MarkerSize = 7

            Dim sql As String = "SELECT CONVERT(date, DateAndTime) AS TranDate, SUM(Gross) AS TotalGross " &
                                "FROM Transactions " &
                                "GROUP BY CONVERT(date, DateAndTime) " &
                                "ORDER BY TranDate"

            Dim dt As DataTable = ExecuteQuery(sql)

            For Each row As DataRow In dt.Rows
                salesSeries.Points.AddXY(Convert.ToDateTime(row("TranDate")), Convert.ToDecimal(row("TotalGross")))
            Next

            Chart1.Series.Add(salesSeries)

            With Chart1.ChartAreas(0)
                .AxisX.Title = "Date"
                .AxisY.Title = "Gross Sales"
                .AxisX.LabelStyle.Format = "MM/dd"
                .AxisX.Interval = 1
                .AxisX.IntervalType = DateTimeIntervalType.Days
            End With

        Catch ex As Exception
            MessageBox.Show("Failed to load chart data: " & ex.Message,
                            "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub CashierDashboard_Activated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        LoadChartFromTransactions()
        Dim salesSummary As Dictionary(Of String, Decimal) = GetSalesSummary()
        LblTodaySales.Text = "₱" & salesSummary("Today").ToString("N2")

        LoadTodaysTransactions(DgvTransactions)

        Dim todayTransactionCount As Integer = GetDgvRowCount(DgvTransactions)
        Guna2HtmlLabel5.Text = todayTransactionCount
    End Sub
    Public Sub LoadTodaysTransactions(ByVal dgv As DataGridView)
        Try
            Dim sql As String = "SELECT TransactionID, DateAndTime, Cashier, Gross, Vat, Total " & _
                                "FROM Transactions " & _
                                "WHERE CONVERT(date, DateAndTime) = @today " & _
                                "ORDER BY DateAndTime DESC"

            Parameters.Clear()
            AddParam("@today", DateTime.Now.Date)

            Dim dt As DataTable = ExecuteQuery(sql)

            If dt IsNot Nothing Then
                dgv.DataSource = dt

                ' Format money columns
                For Each colName In {"Gross", "Vat", "Total"}
                    If dgv.Columns.Contains(colName) Then
                        dgv.Columns(colName).DefaultCellStyle.Format = "₱#,##0.00"
                    End If
                Next
            Else
                dgv.DataSource = Nothing
            End If

        Catch ex As Exception
            MessageBox.Show("Failed to load today's transactions: " & ex.Message,
                            "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Function GetDgvRowCount(ByVal dgv As DataGridView) As Integer
        If dgv.AllowUserToAddRows Then
            Return dgv.Rows.Count - 1
        Else
            Return dgv.Rows.Count
        End If
    End Function

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click
        History.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
       
    End Sub


End Class