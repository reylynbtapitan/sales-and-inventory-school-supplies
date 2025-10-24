Imports MessagingToolkit.Barcode
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Data.SqlClient

Public Class Inventory


    Private Sub Inventory_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        RefreshProductsGrid(DGVProducts)
        Guna2HtmlLabel3.Text = CurrentUser

        LoadCategories()
        LoadCategories1()
    End Sub
    Private Sub LoadCategories1()
        CmbCategory.Items.Clear()
        CmbCategory.Items.AddRange(New String() {
            "Writing Materials",
            "Paper & Notebooks",
            "Dairy Products",
            "Organization & Storage",
            "Art Supplies"
        })
    End Sub

    Private Sub Guna2GradientButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton2.Click
      Try
            ' ✅ Validate required fields
            If String.IsNullOrWhiteSpace(TxtProductName.Text) OrElse
               String.IsNullOrWhiteSpace(CmbCategory.Text) OrElse
               String.IsNullOrWhiteSpace(TxtQuantity.Text) OrElse
               String.IsNullOrWhiteSpace(TxtUnitPrice.Text) OrElse
               String.IsNullOrWhiteSpace(TxtSellingPrice.Text) OrElse
               PictureBox1.Image Is Nothing OrElse
               BarcodePictureBox.Image Is Nothing Then

                MessageBox.Show("Please fill out all fields and upload a photo.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            ' ✅ Numeric validations
            Dim quantity As Integer
            Dim unitPrice As Decimal
            Dim sellingPrice As Decimal

            If Not Integer.TryParse(TxtQuantity.Text, quantity) OrElse quantity < 0 Then
                MessageBox.Show("Quantity must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TxtQuantity.Focus()
                Exit Sub
            End If

            If Not Decimal.TryParse(TxtUnitPrice.Text, unitPrice) OrElse unitPrice < 0 Then
                MessageBox.Show("Unit Price must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TxtUnitPrice.Focus()
                Exit Sub
            End If

            If Not Decimal.TryParse(TxtSellingPrice.Text, sellingPrice) OrElse sellingPrice < 0 Then
                MessageBox.Show("Selling Price must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TxtSellingPrice.Focus()
                Exit Sub
            End If

            ' ✅ Check for duplicate product
            Dim checkSql As String = "SELECT COUNT(*) FROM Products WHERE LOWER(ProductName) = LOWER(@ProductName)"
            AddParam("@ProductName", TxtProductName.Text.Trim())
            Dim dt As DataTable = ExecuteQuery(checkSql)
            If dt.Rows.Count > 0 AndAlso Convert.ToInt32(dt.Rows(0)(0)) > 0 Then
                MessageBox.Show("A product with the same name already exists. Please use a different name.", "Duplicate Product", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            ' ✅ Save product image
            Dim saveFolder As String = Path.Combine(Application.StartupPath, "ProductImages")
            If Not Directory.Exists(saveFolder) Then Directory.CreateDirectory(saveFolder)
            Dim photoFileName As String = Guid.NewGuid().ToString() & ".jpg"
            Dim photoPath As String = Path.Combine(saveFolder, photoFileName)
            PictureBox1.Image.Save(photoPath, Imaging.ImageFormat.Jpeg)

            ' ✅ Save barcode
            Dim desktopPath As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            Dim productBarcodeFolder As String = Path.Combine(desktopPath, "Barcodes", TxtProductName.Text.Trim() & "_" & DateTime.Now.ToString("yyyyMMddHHmmss"))
            If Not Directory.Exists(productBarcodeFolder) Then Directory.CreateDirectory(productBarcodeFolder)
            Dim originalBarcodePath As String = BarcodePictureBox.Tag.ToString()
            Dim newBarcodePath As String = Path.Combine(productBarcodeFolder, Path.GetFileName(originalBarcodePath))
            File.Copy(originalBarcodePath, newBarcodePath, True)
            Dim barcodePath As String = newBarcodePath

            ' ✅ Build barcode text
            Dim barcodeText As String = String.Join("|", {
                TxtProductName.Text.Trim(),
                CmbCategory.Text.Trim(),
                quantity.ToString(),
                unitPrice.ToString("F2"),
                sellingPrice.ToString("F2")
            })

            ' ✅ Insert query (removed ExpiryDate)
            Dim sql As String = "INSERT INTO Products " &
                "(ProductName, Category, Quantity, UnitPrice, SellingPrice, Barcode, BarcodePath, PicturePhotoPath, DateAdded) " &
                "VALUES (@ProductName, @Category, @Quantity, @UnitPrice, @SellingPrice, @Barcode, @BarcodePath, @PhotoPath, GETDATE())"

      ' ✅ Add parameters
            AddParam("@ProductName", TxtProductName.Text.Trim())
            AddParam("@Category", CmbCategory.Text.Trim())
            AddParam("@Quantity", quantity)
            AddParam("@UnitPrice", unitPrice)
            AddParam("@SellingPrice", sellingPrice)
            AddParam("@Barcode", barcodeText)
            AddParam("@BarcodePath", barcodePath)
            AddParam("@PhotoPath", photoPath)

            ' ✅ Execute
            If ExecuteNonQuery(sql) Then
                MessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                RefreshProductsGrid(DGVProducts)
                ClearInputs()
                LoadCategories()
            Else
                MessageBox.Show("Failed to add product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
     
    End Sub
    Private Sub ClearInputs()
        TxtProductID.Clear()
        TxtProductName.Clear()
        CmbCategory.SelectedIndex = -1
        TxtQuantity.Clear()
        TxtUnitPrice.Clear()
        TxtSellingPrice.Clear()
        PictureBox1.Image = Nothing
        PictureBox1.Tag = Nothing
        BarcodePictureBox.Image = Nothing
        BarcodePictureBox.Tag = Nothing
    End Sub
    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            ofd.Title = "Select Product Photo"

            If ofd.ShowDialog() = DialogResult.OK Then
                PictureBox1.Image = Image.FromFile(ofd.FileName)
                PictureBox1.Tag = ofd.FileName
            End If
        End Using
    End Sub
    Private Sub Guna2GradientButton11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton11.Click
        ClearInputs()
    End Sub
    Private Sub Inventory_Activated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        RefreshProductsGrid(DGVProducts)
        Guna2HtmlLabel19.Text = CurrentUser
        LoadCategories1()
    End Sub
    Private Sub InputChanged(ByVal sender As Object, ByVal e As EventArgs) _
        Handles TxtProductName.TextChanged, CmbCategory.SelectedIndexChanged,
                TxtQuantity.TextChanged, TxtUnitPrice.TextChanged, TxtSellingPrice.TextChanged

        If AreInputsComplete() Then
            GenerateProductBarcode()
        Else
            BarcodePictureBox.Image = Nothing
            BarcodePictureBox.Tag = Nothing
        End If
    End Sub
    Private Function AreInputsComplete() As Boolean
        Return Not (String.IsNullOrWhiteSpace(TxtProductName.Text) OrElse
                    String.IsNullOrWhiteSpace(CmbCategory.Text) OrElse
                    String.IsNullOrWhiteSpace(TxtQuantity.Text) OrElse
                    String.IsNullOrWhiteSpace(TxtUnitPrice.Text) OrElse
                    String.IsNullOrWhiteSpace(TxtSellingPrice.Text))
    End Function
    Private Sub GenerateProductBarcode()
        Try
            Dim barcodeText As String = String.Join("|", {
                TxtProductName.Text.Trim()
            })
            Dim barcodeGenerator As New BarcodeEncoder()
            barcodeGenerator.IncludeLabel = True
            barcodeGenerator.LabelFont = New Font("Arial", 9, FontStyle.Regular)
            barcodeGenerator.CustomLabel = TxtProductName.Text.Trim()
            barcodeGenerator.Alignment = StringAlignment.Center

            Dim rawBarcode As Bitmap = barcodeGenerator.Encode(BarcodeFormat.Code128, barcodeText)

            Dim margin As Integer = 10
            Dim newWidth As Integer = rawBarcode.Width + (margin * 2)
            Dim newHeight As Integer = rawBarcode.Height + (margin * 2)

            Dim finalBarcode As New Bitmap(newWidth, newHeight)
            Using g As Graphics = Graphics.FromImage(finalBarcode)
                g.Clear(Color.White) 
                g.DrawImage(rawBarcode, margin, margin)
            End Using

            BarcodePictureBox.Image = finalBarcode

            Dim folder As String = Path.Combine(Application.StartupPath, "System Barcode")
            If Not Directory.Exists(folder) Then Directory.CreateDirectory(folder)

            Dim filename As String = TxtProductName.Text.Trim() & "_" &
                                      DateTime.Now.ToString("yyyyMMddHHmmss") & ".png"
            Dim fullPath As String = Path.Combine(folder, filename)

            finalBarcode.Save(fullPath, Imaging.ImageFormat.Png)
            BarcodePictureBox.Tag = fullPath

        Catch ex As Exception
            BarcodePictureBox.Image = Nothing
            BarcodePictureBox.Tag = Nothing
            MessageBox.Show("Error generating barcode: " & ex.Message,
                            "Barcode Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub DGVProducts_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGVProducts.CellClick
       Try
            If e.RowIndex < 0 Then Exit Sub
            Dim row As DataGridViewRow = DGVProducts.Rows(e.RowIndex)

            ' Load main product fields
            TxtProductID.Text = row.Cells("ProductID").Value.ToString()
            TxtProductName.Text = row.Cells("ProductName").Value.ToString()
            CmbCategory.Text = row.Cells("Category").Value.ToString()
            TxtQuantity.Text = row.Cells("Quantity").Value.ToString()
            TxtUnitPrice.Text = row.Cells("UnitPrice").Value.ToString()
            TxtSellingPrice.Text = row.Cells("SellingPrice").Value.ToString()

            ' Load product details from database
            Dim productId As Integer = Convert.ToInt32(row.Cells("ProductID").Value)
            Dim sql As String = "" &
                "SELECT PicturePhotoPath, BarcodePath, ExpiryDate " & vbCrLf &
                "FROM Products " & vbCrLf &
                "WHERE ProductID = @ProductID"
            AddParam("@ProductID", productId)
            Dim dt As DataTable = ExecuteQuery(sql)

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim photoPath As String = dt.Rows(0)("PicturePhotoPath").ToString()
                Dim barcodePath As String = dt.Rows(0)("BarcodePath").ToString()
                Dim expiryObj As Object = dt.Rows(0)("ExpiryDate")

                ' Load product image
                If IO.File.Exists(photoPath) Then
                    PictureBox1.Image = Image.FromFile(photoPath)
                    PictureBox1.Tag = photoPath
                Else
                    PictureBox1.Image = Nothing
                    PictureBox1.Tag = Nothing
                End If

                ' Load barcode image
                If IO.File.Exists(barcodePath) Then
                    BarcodePictureBox.Image = Image.FromFile(barcodePath)
                    BarcodePictureBox.Tag = barcodePath
                Else
                    BarcodePictureBox.Image = Nothing
                    BarcodePictureBox.Tag = Nothing
                End If

               

            End If

        Catch ex As Exception
            MessageBox.Show("Error loading product details: " & ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub Guna2GradientButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton9.Click
        Try
            ' ✅ Validate that a product is selected
            If String.IsNullOrWhiteSpace(TxtProductID.Text) Then
                MessageBox.Show("Please select a product to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            ' ✅ Validate required fields
            If String.IsNullOrWhiteSpace(TxtProductName.Text) OrElse
               String.IsNullOrWhiteSpace(CmbCategory.Text) OrElse
               String.IsNullOrWhiteSpace(TxtQuantity.Text) OrElse
               String.IsNullOrWhiteSpace(TxtUnitPrice.Text) OrElse
               String.IsNullOrWhiteSpace(TxtSellingPrice.Text) Then

                MessageBox.Show("Please fill out all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            ' ✅ Numeric validation
            Dim quantity As Integer
            Dim unitPrice As Decimal
            Dim sellingPrice As Decimal

            If Not Integer.TryParse(TxtQuantity.Text, quantity) OrElse quantity < 0 Then
                MessageBox.Show("Quantity must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TxtQuantity.Focus()
                Exit Sub
            End If

            If Not Decimal.TryParse(TxtUnitPrice.Text, unitPrice) OrElse unitPrice < 0 Then
                MessageBox.Show("Unit Price must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TxtUnitPrice.Focus()
                Exit Sub
            End If

            If Not Decimal.TryParse(TxtSellingPrice.Text, sellingPrice) OrElse sellingPrice < 0 Then
                MessageBox.Show("Selling Price must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TxtSellingPrice.Focus()
                Exit Sub
            End If

            ' ✅ Update image if new one is selected
            Dim photoPath As String = Nothing
            If PictureBox1.Image IsNot Nothing Then
                Dim saveFolder As String = Path.Combine(Application.StartupPath, "ProductImages")
                If Not Directory.Exists(saveFolder) Then Directory.CreateDirectory(saveFolder)
                Dim photoFileName As String = Guid.NewGuid().ToString() & ".jpg"
                photoPath = Path.Combine(saveFolder, photoFileName)
                PictureBox1.Image.Save(photoPath, Imaging.ImageFormat.Jpeg)
            End If

            ' ✅ Build UPDATE query
            Dim sql As String =
                "UPDATE Products SET " &
                "ProductName = @ProductName, " &
                "Category = @Category, " &
                "Quantity = @Quantity, " &
                "UnitPrice = @UnitPrice, " &
                "SellingPrice = @SellingPrice, " &
                If(photoPath IsNot Nothing, ", PicturePhotoPath = @PhotoPath", "") &
                " WHERE ProductID = @ProductID"

            ' ✅ Add parameters
            AddParam("@ProductID", Convert.ToInt32(TxtProductID.Text))
            AddParam("@ProductName", TxtProductName.Text.Trim())
            AddParam("@Category", CmbCategory.Text.Trim())
            AddParam("@Quantity", quantity)
            AddParam("@UnitPrice", unitPrice)
            AddParam("@SellingPrice", sellingPrice)
            If photoPath IsNot Nothing Then AddParam("@PhotoPath", photoPath)

            ' ✅ Execute
            If ExecuteNonQuery(sql) Then
                MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Refresh the DataGridView in Inventory form if open
                If Application.OpenForms().OfType(Of Inventory).Any() Then
                    Dim inv As Inventory = Application.OpenForms().OfType(Of Inventory).First()
                    inv.RefreshProductsGrid(inv.DGVProducts)
                End If

                ClearInputs()
                LoadCategories()
            Else
                MessageBox.Show("Failed to update product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show("An error occurred while updating: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadCategories()
        Try
            Dim dt As DataTable = ExecuteQuery("SELECT DISTINCT Category FROM Products")
            CmbFilterCategory.Items.Clear()
            CmbFilterCategory.Items.Add("All") ' Add "All" option

            For Each row As DataRow In dt.Rows
                CmbFilterCategory.Items.Add(row("Category").ToString())
            Next

            CmbFilterCategory.SelectedIndex = 0 ' Default to All
        Catch ex As Exception
            MessageBox.Show("Error loading categories: " & ex.Message)
        End Try
    End Sub
    Private Sub RefreshProductsGrid(ByVal dgv As DataGridView)
        Try
            Dim sql As String = "SELECT ProductID, ProductName, Category, Quantity, UnitPrice, SellingPrice, BarcodePath, PicturePhotoPath FROM Products WHERE 1=1"

            If Not String.IsNullOrWhiteSpace(TxtSearch.Text) Then
                sql &= " AND ProductName LIKE @Search"
                AddParam("@Search", "%" & TxtSearch.Text.Trim() & "%")
            End If

            If CmbFilterCategory.SelectedItem IsNot Nothing AndAlso CmbFilterCategory.SelectedItem.ToString() <> "All" Then
                sql &= " AND Category = @Category"
                AddParam("@Category", CmbFilterCategory.SelectedItem.ToString())
            End If

            Dim dt As DataTable = ExecuteQuery(sql)
            dgv.DataSource = dt

            If dgv.Columns.Contains("PicturePhotoPath") Then dgv.Columns("PicturePhotoPath").Visible = False
            If dgv.Columns.Contains("BarcodePath") Then dgv.Columns("BarcodePath").Visible = False

            Dim outOfStock As Integer = 0
            Dim lowStock As Integer = 0

            For Each row As DataRow In dt.Rows
                Dim qty As Integer = Convert.ToInt32(row("Quantity"))
                If qty = 0 Then
                    outOfStock += 1
                ElseIf qty <= 50 Then
                    lowStock += 1
                End If
            Next

            LblOutOfStock.Text = "0" & outOfStock.ToString()
            LblLowStock.Text = "0" & lowStock.ToString()
            LblTotalItems.Text = "0" & dt.Rows.Count.ToString()
            DGVProducts.Refresh()

        Catch ex As Exception
            MessageBox.Show("Error loading products: " & ex.Message)
        End Try
    End Sub
    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtSearch.TextChanged
        RefreshProductsGrid(DGVProducts)
    End Sub
    Private Sub CmbFilterCategory_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmbFilterCategory.SelectedIndexChanged
        RefreshProductsGrid(DGVProducts)
    End Sub
    Private Sub DGVProducts_CellFormatting(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellFormattingEventArgs) Handles DGVProducts.CellFormatting
        Try
            If DGVProducts.Columns(e.ColumnIndex).Name = "Quantity" AndAlso e.Value IsNot Nothing Then
                Dim qty As Integer = Convert.ToInt32(e.Value)

                If qty = 0 Then
                    DGVProducts.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.Red
                    DGVProducts.Rows(e.RowIndex).DefaultCellStyle.ForeColor = Color.White
                ElseIf qty <= 50 Then
                    DGVProducts.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.Yellow
                    DGVProducts.Rows(e.RowIndex).DefaultCellStyle.ForeColor = Color.Black
                Else
                    DGVProducts.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.White
                    DGVProducts.Rows(e.RowIndex).DefaultCellStyle.ForeColor = Color.Black
                End If
            End If
        Catch ex As Exception
            Console.WriteLine("Error formatting rows: " & ex.Message)
        End Try
    End Sub
    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        GlobalLogout()
    End Sub
    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    
    End Sub
    Private Sub Guna2GradientButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
      
    End Sub


    Private Sub DGVProducts_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGVProducts.CellContentClick

    End Sub

    Private Sub Guna2GradientButton10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If String.IsNullOrWhiteSpace(TxtProductID.Text) Then
            MessageBox.Show("Please select a product first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        pnlOrderRequest.Visible = True
    End Sub
    Private Sub Guna2GradientButton13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton13.Click
        pnlOrderRequest.Visible = False
    End Sub

    Private Sub Guna2Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2Button1.Click
        Try
            ' Validate product info
            If String.IsNullOrWhiteSpace(TxtProductName.Text) Then
                MessageBox.Show("Product information missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim productName As String = TxtProductName.Text.Trim()
            Dim category As String = CmbCategory.Text.Trim()
            Dim quantity As Integer = CInt(nudOrderQuantity.Value)

            If quantity <= 0 Then
                MessageBox.Show("Please enter a valid quantity to order.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            ' Serialize details (same logic as your ReturnItem)
            Dim orderDetails As String = String.Format("{0}|{1}|{2}|{3}",
                                                       productName,
                                                       category,
                                                       quantity)

            ' === Check if a request for this product already exists ===
            Dim checkSql As String = "SELECT Status FROM ApprovalRequests WHERE RequestType='OrderItem' AND Details=@Details"
            AddParam("@Details", orderDetails)
            Dim dt As DataTable = ExecuteQuery(checkSql)

            If dt.Rows.Count = 0 Then
                ' === No existing request → create new one ===
                Dim insertSql As String = "INSERT INTO ApprovalRequests (RequestType, RequestedBy, Details, Status, DateRequested) " &
                                          "VALUES ('OrderItem', @RequestedBy, @Details, 'Pending', GETDATE())"
                AddParam("@RequestedBy", CurrentUser)
                AddParam("@Details", orderDetails)

                If ExecuteNonQuery(insertSql) Then
                    MessageBox.Show("Order request sent to manager for approval.", "Request Sent", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    pnlOrderRequest.Visible = False
                Else
                    MessageBox.Show("Failed to send order request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If

            Else
                ' === Request already exists — check its status ===
                Dim status As String = dt.Rows(0)("Status").ToString().ToLower()
                Select Case status
                    Case "pending"
                        MessageBox.Show("An order request for this product is still pending approval.", "Pending Request", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    Case "rejected"
                        If MessageBox.Show("Previous order request was rejected. Send again?", "Request Rejected", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                            Dim updateSql As String = "UPDATE ApprovalRequests SET Status='Pending', DateRequested=GETDATE() " &
                                                      "WHERE RequestType='OrderItem' AND Details=@Details"
                            AddParam("@Details", orderDetails)
                            ExecuteNonQuery(updateSql)
                            MessageBox.Show("Order request re-sent for approval.", "Request Re-Sent", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If

                    Case "approved"
                        MessageBox.Show("This order request has already been approved.", "Approved", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Select
            End If

        Catch ex As Exception
            MessageBox.Show("Error sending order request: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Guna2GradientButton6_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton6.Click
        InventoryDashboard.Show()
        Me.Hide()
    End Sub



    Private Sub Guna2GradientButton5_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub

    Private Sub Guna2HtmlLabel3_Click(sender As System.Object, e As System.EventArgs) Handles Guna2HtmlLabel3.Click

    End Sub

    Private Sub Guna2PictureBox1_Click(sender As System.Object, e As System.EventArgs)

    End Sub

    Private Sub pnlOrderRequest_Paint(sender As System.Object, e As System.Windows.Forms.PaintEventArgs) Handles pnlOrderRequest.Paint

    End Sub
End Class