Imports System.IO
Imports System.Drawing
Public Class MInventory

    Private Sub Guna2GradientButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub
    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub MInventory_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            ' Set date and current user
            Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
            Guna2HtmlLabel19.Text = CurrentUser

            ' Load categories, products, and stock counts
            LoadCategories()
            RefreshProductsGrid()
            CountStocks()
        Catch ex As Exception
            MessageBox.Show("Error loading inventory: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub RefreshProductsGrid()
        Try
            Dim sql As String = "SELECT ProductID, ProductName, Category, Quantity, UnitPrice, SellingPrice, PicturePhotoPath, BarcodePath FROM Products WHERE 1=1"

            ' Search filter
            If Not String.IsNullOrWhiteSpace(TxtSearch.Text) Then
                sql &= " AND ProductName LIKE @Search"
                AddParam("@Search", "%" & TxtSearch.Text.Trim() & "%")
            End If

            ' Category filter
            If CmbCategoryFilter.SelectedItem IsNot Nothing AndAlso CmbCategoryFilter.SelectedItem.ToString() <> "All" Then
                sql &= " AND Category = @Category"
                AddParam("@Category", CmbCategoryFilter.SelectedItem.ToString())
            End If

            Dim dt As DataTable = ExecuteQuery(sql)
            If dt IsNot Nothing Then
                DGVProducts.AutoGenerateColumns = True
                DGVProducts.DataSource = dt

                ' Hide image/barcode columns
                If DGVProducts.Columns.Contains("PicturePhotoPath") Then DGVProducts.Columns("PicturePhotoPath").Visible = False
                If DGVProducts.Columns.Contains("BarcodePath") Then DGVProducts.Columns("BarcodePath").Visible = False

                DGVProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                DGVProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect
                DGVProducts.ReadOnly = True
            End If

            CountStocks()
        Catch ex As Exception
            MessageBox.Show("Error loading products: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LoadCategories()
        Try
            Dim dt As DataTable = ExecuteQuery("SELECT DISTINCT Category FROM Products ORDER BY Category")
            CmbCategoryFilter.Items.Clear()
            CmbCategoryFilter.Items.Add("All")

            For Each row As DataRow In dt.Rows
                CmbCategoryFilter.Items.Add(row("Category").ToString())
            Next

            CmbCategoryFilter.SelectedIndex = 0
        Catch ex As Exception
            MessageBox.Show("Error loading categories: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub CountStocks()
        Try
            Dim dt As DataTable = ExecuteQuery("SELECT Quantity FROM Products")
            Dim total As Integer = If(dt IsNot Nothing, dt.Rows.Count, 0)
            Dim lowStock As Integer = 0
            Dim outOfStock As Integer = 0

            If dt IsNot Nothing Then
                For Each row As DataRow In dt.Rows
                    Dim qty As Integer = Convert.ToInt32(row("Quantity"))
                    If qty = 0 Then
                        outOfStock += 1
                    ElseIf qty <= 50 Then
                        lowStock += 1
                    End If
                Next
            End If

            LblTotalItems.Text = total.ToString()
            LblLowStock.Text = lowStock.ToString()
            LblOutOfStock.Text = outOfStock.ToString()
        Catch ex As Exception
            MessageBox.Show("Error counting stocks: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub DgvTransactions_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs)
        Try
            If e.RowIndex < 0 Then Exit Sub
            Dim row As DataGridViewRow = DGVProducts.Rows(e.RowIndex)

            TxtProductID.Text = row.Cells("ProductID").Value.ToString()
            TxtProductName.Text = row.Cells("ProductName").Value.ToString()
            TxtCategory.Text = row.Cells("Category").Value.ToString()
            TxtQuantity.Text = row.Cells("Quantity").Value.ToString()
            TxtUnitPrice.Text = row.Cells("UnitPrice").Value.ToString()
            TxtSellingPrice.Text = row.Cells("SellingPrice").Value.ToString()

            ' Load image
            Dim photoPath As String = row.Cells("PicturePhotoPath").Value.ToString()
            If Not String.IsNullOrWhiteSpace(photoPath) AndAlso File.Exists(photoPath) Then
                PictureBoxPhoto.Image = Image.FromFile(photoPath)
            Else
                PictureBoxPhoto.Image = Nothing
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading product info: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RefreshProductsGrid()
    End Sub

    Private Sub Guna2ComboBoxCategory_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RefreshProductsGrid()
    End Sub

    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ManagerDashboard.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
   
    End Sub

    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        MAccounts.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        MSales.Show()
        Me.Hide()
    End Sub

    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        GlobalLogout()
    End Sub
End Class