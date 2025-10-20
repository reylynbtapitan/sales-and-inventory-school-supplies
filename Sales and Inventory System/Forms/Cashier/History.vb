Imports System.Data.SqlClient

Public Class History

    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton6.Click
        Me.Hide()
        CashierDashboard.Show()
    End Sub
    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton4.Click
        Sales.Show()
        Me.Hide()
    End Sub
    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton3.Click
        Me.Hide()
        CashierInventory.Show()
    End Sub
    Private Sub History_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        Guna2HtmlLabel19.Text = CurrentUser
        LoadTransactions()

        Dim salesSummary = GetSalesSummary()

        LblTodaySales.Text = salesSummary("Today").ToString("N2")
        LblYesterdaySales.Text = salesSummary("Yesterday").ToString("N2")
        LblAccumulatedSales.Text = salesSummary("Accumulated").ToString("N2")
    End Sub
    Private Sub LoadTransactions(Optional ByVal filterDate As Date? = Nothing, Optional ByVal searchText As String = "")
        Try
            If DgvTransactions Is Nothing Then
                MessageBox.Show("Transactions DataGridView is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            Dim sql As String = "SELECT TransactionID, DateAndTime, Cashier, Gross, Vat, Total FROM Transactions WHERE 1=1"

            Parameters.Clear()

            If filterDate.HasValue Then
                sql &= " AND CONVERT(date, DateAndTime) = @dateFilter"
                AddParam("@dateFilter", filterDate.Value.Date)
            End If

            If Not String.IsNullOrWhiteSpace(searchText) Then
                sql &= " AND (Cashier LIKE @search OR CAST(TransactionID AS NVARCHAR) LIKE @search)"
                AddParam("@search", "%" & searchText.Trim() & "%")
            End If

            sql &= " ORDER BY DateAndTime DESC"

            Dim dt As DataTable = ExecuteQuery(sql)

            DgvTransactions.DataSource = Nothing
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                DgvTransactions.DataSource = dt

                If DgvTransactions.Columns.Contains("TransactionID") Then
                    DgvTransactions.Columns("TransactionID").HeaderText = "ID"
                End If
                If DgvTransactions.Columns.Contains("DateAndTime") Then
                    DgvTransactions.Columns("DateAndTime").HeaderText = "Date & Time"
                End If
                If DgvTransactions.Columns.Contains("Cashier") Then
                    DgvTransactions.Columns("Cashier").HeaderText = "Cashier"
                End If
                If DgvTransactions.Columns.Contains("Gross") Then
                    DgvTransactions.Columns("Gross").DefaultCellStyle.Format = "N2"
                End If
                If DgvTransactions.Columns.Contains("Vat") Then
                    DgvTransactions.Columns("Vat").DefaultCellStyle.Format = "N2"
                End If
                If DgvTransactions.Columns.Contains("Total") Then
                    DgvTransactions.Columns("Total").DefaultCellStyle.Format = "N2"
                End If

                DgvTransactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            End If

        Catch ex As Exception
            MessageBox.Show("Failed to load transactions: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtSearch.TextChanged
        LoadTransactions(DateTimePicker1.Value, TxtSearch.Text)
    End Sub
    Private Sub DateTimePicker1_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DateTimePicker1.ValueChanged
        LoadTransactions(DateTimePicker1.Value, TxtSearch.Text)
    End Sub
    
    
  

    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub

    Private Sub Guna2HtmlLabel17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2HtmlLabel17.Click

    End Sub
End Class