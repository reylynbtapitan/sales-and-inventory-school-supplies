Public Class InventoryDashboard

    Private Sub Guna2GradientPanel1_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs)

    End Sub


    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub

    Private Sub InventoryDashboard_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        Guna2HtmlLabel19.Text = CurrentUser
        Guna2HtmlLabel2.Text = CurrentUser
        LoadLowAndOutOfStock()
        LoadTotalItems()

    End Sub
    Private Sub LoadTotalItems()
        Try
            Dim sql As String = "SELECT COUNT(*) FROM Products"
            Dim dt As DataTable = ExecuteQuery(sql)
            If dt.Rows.Count > 0 Then
                LblTotalItems.Text = "Total Items: " & dt.Rows(0)(0).ToString()
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading total items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton4.Click
        Inventory.Show()
        Me.Hide()
    End Sub

    Private Sub LoadLowAndOutOfStock(Optional ByVal search As String = "")
        Try
            Dim sql As String = "" &
                "SELECT ProductID, ProductName, Category, Quantity, UnitPrice, SellingPrice, Supplier " & vbCrLf &
                "FROM Products " & vbCrLf &
                "WHERE Quantity <= 50 " ' both low and out-of-stock

            If Not String.IsNullOrWhiteSpace(search) Then
                sql &= " AND ProductName LIKE @Search"
                AddParam("@Search", "%" & search.Trim() & "%")
            End If

            sql &= " ORDER BY Quantity ASC"

            Dim dt As DataTable = ExecuteQuery(sql)
            DGVLowStock.DataSource = dt

            ' Hide unnecessary columns if needed
            If DGVLowStock.Columns.Contains("Supplier") Then
                DGVLowStock.Columns("Supplier").Width = 150
            End If

            ' Apply color formatting
            For Each row As DataGridViewRow In DGVLowStock.Rows
                Dim qty As Integer = Convert.ToInt32(row.Cells("Quantity").Value)
                If qty = 0 Then
                    row.DefaultCellStyle.BackColor = Color.Red
                    row.DefaultCellStyle.ForeColor = Color.White
                ElseIf qty <= 50 Then
                    row.DefaultCellStyle.BackColor = Color.Yellow
                    row.DefaultCellStyle.ForeColor = Color.Black
                End If
            Next

            ' Count values
            Dim lowCount As Integer = dt.Select("Quantity > 0 AND Quantity <= 50").Length
            Dim outCount As Integer = dt.Select("Quantity = 0").Length

            If LblLowCount IsNot Nothing Then
                LblLowCount.Text = "Low Stock: " & lowCount.ToString()
            End If
            If LblOutCount IsNot Nothing Then
                LblOutCount.Text = "Out of Stock: " & outCount.ToString()
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading low/out-of-stock products: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click
        Me.Hide()
        Inventory.Show()
    End Sub

    ' ✅ Setup DataGridView Columns
    Private Sub SetupDGV(ByVal dgv As DataGridView)
        dgv.Columns.Clear()
        dgv.AutoGenerateColumns = False

        dgv.Columns.Add("RequestID", "Request ID")
        dgv.Columns("RequestID").DataPropertyName = "RequestID"
        dgv.Columns("RequestID").Visible = False

        dgv.Columns.Add("RequestType", "Request Type")
        dgv.Columns("RequestType").DataPropertyName = "RequestType"

        dgv.Columns.Add("RequestedBy", "Requested By")
        dgv.Columns("RequestedBy").DataPropertyName = "RequestedBy"

        dgv.Columns.Add("Details", "Details")
        dgv.Columns("Details").DataPropertyName = "Details"
        dgv.Columns("Details").Width = 250

        dgv.Columns.Add("Status", "Status")
        dgv.Columns("Status").DataPropertyName = "Status"

        dgv.Columns.Add("DeliveryStatus", "Delivery Status")
        dgv.Columns("DeliveryStatus").DataPropertyName = "DeliveryStatus"

        dgv.Columns.Add("DateRequested", "Date Requested")
        dgv.Columns("DateRequested").DataPropertyName = "DateRequested"
        dgv.Columns("DateRequested").DefaultCellStyle.Format = "yyyy-MM-dd HH:mm"
        dgv.Columns("DateRequested").Width = 150

        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
    End Sub

    Private Sub Guna2HtmlLabel17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2HtmlLabel17.Click

    End Sub

    Private Sub DGVRequests_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs)

    End Sub
End Class
