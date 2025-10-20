Imports System.Data.SqlClient
Public Class Sales

    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton6.Click
        CashierDashboard.Show()
        Me.Hide()
    End Sub
    Private Sub Sales_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        Guna2HtmlLabel19.Text = CurrentUser
        LoadProductsToFlow()
        LoadCategories()

        If DgvCart.Columns.Count = 0 Then
            DgvCart.Columns.Add("ID", "ID")
            DgvCart.Columns.Add("ProductName", "Product Name")
            DgvCart.Columns.Add("Quantity", "Quantity")
            DgvCart.Columns.Add("Price", "Price")
            DgvCart.Columns.Add("Total", "Total")
        End If

    End Sub
    Private Sub LoadProductsToFlow()
        FlowLayoutPanel1.Controls.Clear()

        ' Safely get search text
        Dim searchText As String = ""
        If TxtSearch IsNot Nothing Then searchText = TxtSearch.Text.Trim()

        ' Safely get selected category
        Dim selectedCategory As String = "All"
        If Guna2ComboBoxCategory.SelectedItem IsNot Nothing Then
            selectedCategory = Guna2ComboBoxCategory.SelectedItem.ToString()
        End If

        ' Build SQL query
        Dim sql As String = "SELECT ProductID, ProductName, Quantity, SellingPrice, PicturePhotoPath, Category FROM Products WHERE Quantity > 0"

        ' Apply category filter if not "All"
        If selectedCategory <> "All" Then
            sql &= " AND Category = @Category"
            AddParam("@Category", selectedCategory)
        End If

        ' Apply search filter
        If Not String.IsNullOrEmpty(searchText) Then
            sql &= " AND ProductName LIKE @Search"
            AddParam("@Search", "%" & searchText & "%")
        End If

        ' Run query
        If Query(sql) AndAlso Data IsNot Nothing AndAlso Data.Tables.Count > 0 AndAlso Data.Tables(0).Rows.Count > 0 Then
            For Each row As DataRow In Data.Tables(0).Rows
                Dim photoPath As String = row("PicturePhotoPath").ToString()
                If IO.File.Exists(photoPath) Then
                    ' Panel container
                    Dim itemPanel As New Guna.UI2.WinForms.Guna2GradientPanel() With {
                        .Width = Guna2GradientPanel2.Width,
                        .Height = Guna2GradientPanel2.Height - 10,
                        .BorderColor = Color.FromArgb(159, 1, 1),
                        .BorderThickness = 2,
                        .BorderStyle = Drawing2D.DashStyle.Solid,
                        .BorderRadius = 15
                    }

                    ' Product image
                    Dim pb As New Guna.UI2.WinForms.Guna2PictureBox() With {
                        .Image = Image.FromFile(photoPath),
                        .Size = Guna2PictureBox1.Size,
                        .SizeMode = Guna2PictureBox1.SizeMode,
                        .Top = 20,
                        .Left = (itemPanel.Width - Guna2PictureBox1.Width) \ 2
                    }

                    Dim labelOffset As Integer = 23

                    ' Product Name label
                    Dim lblName As New Guna.UI2.WinForms.Guna2HtmlLabel() With {
                        .Text = row("ProductName").ToString(),
                        .Font = Guna2HtmlLabel2.Font,
                        .ForeColor = Guna2HtmlLabel2.ForeColor,
                        .Width = Guna2HtmlLabel2.Width,
                        .Top = pb.Bottom + 5,
                        .Left = 5 + labelOffset
                    }

                    ' Stock label
                    Dim lblStock As New Guna.UI2.WinForms.Guna2HtmlLabel() With {
                        .Text = "Stock: " & row("Quantity").ToString(),
                        .Font = Guna2HtmlLabel3.Font,
                        .ForeColor = Guna2HtmlLabel3.ForeColor,
                        .Width = Guna2HtmlLabel3.Width,
                        .Top = lblName.Bottom,
                        .Left = 5 + labelOffset
                    }

                    ' Price label
                    Dim lblPrice As New Guna.UI2.WinForms.Guna2HtmlLabel() With {
                        .Text = "Price: ₱" & Convert.ToDecimal(row("SellingPrice")).ToString("F2"),
                        .Font = Guna2HtmlLabel9.Font,
                        .ForeColor = Guna2HtmlLabel9.ForeColor,
                        .Width = Guna2HtmlLabel9.Width,
                        .Top = lblStock.Bottom,
                        .Left = 5 + labelOffset
                    }

                    ' Add to Cart Button (no function yet)
                    Dim btnSelect As New Guna.UI2.WinForms.Guna2Button() With {
                        .Text = "Add to Cart",
                        .Font = Guna2Button1.Font,
                        .FillColor = Guna2Button1.FillColor,
                        .ForeColor = Guna2Button1.ForeColor,
                        .Size = Guna2Button1.Size,
                        .Top = lblPrice.Bottom + 15,
                        .Left = (itemPanel.Width - Guna2Button1.Width) \ 2,
                        .BorderRadius = 10
                    }

                    ' Hover effect
                    AddHandler btnSelect.MouseEnter, Sub() btnSelect.FillColor = Color.FromArgb(255, 31, 31)
                    AddHandler btnSelect.MouseLeave, Sub() btnSelect.FillColor = Guna2Button1.FillColor

                    Dim currentRow As DataRow = row
                    Dim currentImage As Image = pb.Image

                    AddHandler btnSelect.Click,
                        Sub()
                            Dim qtyForm As New Quantity()
                            qtyForm.ProductID = currentRow("ProductID").ToString()
                            qtyForm.ProdName = currentRow("ProductName").ToString()
                            qtyForm.ProdStock = Convert.ToInt32(currentRow("Quantity"))
                            qtyForm.ProdPrice = Convert.ToDecimal(currentRow("SellingPrice"))
                            qtyForm.ProdImage = currentImage

                            If qtyForm.ShowDialog() = DialogResult.OK Then
                                Dim selectedQty As Integer = qtyForm.SelectedQuantity
                                Dim total As Decimal = qtyForm.ProdPrice * selectedQty
                                Dim itemExists As Boolean = False

                                For Each dgvRow As DataGridViewRow In DgvCart.Rows
                                    If dgvRow.Cells("ID").Value IsNot Nothing AndAlso
                                       dgvRow.Cells("ID").Value.ToString() = qtyForm.ProductID Then

                                        Dim currentQty As Integer = Convert.ToInt32(dgvRow.Cells("Quantity").Value)
                                        Dim newQty As Integer = currentQty + selectedQty

                                        If newQty > qtyForm.ProdStock Then
                                            MessageBox.Show("The maximum stock for this item is " & qtyForm.ProdStock,
                                                            "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                            newQty = qtyForm.ProdStock
                                        End If

                                        dgvRow.Cells("Quantity").Value = newQty
                                        dgvRow.Cells("Total").Value = (newQty * qtyForm.ProdPrice).ToString("F2")

                                        itemExists = True
                                        Exit For
                                    End If
                                Next

                                If Not itemExists Then
                                    If selectedQty > qtyForm.ProdStock Then
                                        MessageBox.Show("The maximum stock for this item is " & qtyForm.ProdStock,
                                                        "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                        selectedQty = qtyForm.ProdStock
                                    End If

                                    DgvCart.Rows.Add(qtyForm.ProductID,
                                                     qtyForm.ProdName,
                                                     selectedQty,
                                                     qtyForm.ProdPrice.ToString("F2"),
                                                     (qtyForm.ProdPrice * selectedQty).ToString("F2"))
                                End If
                                UpdateCartTotal()
                            End If
                        End Sub

                    ' Add controls to panel
                    itemPanel.Controls.Add(pb)
                    itemPanel.Controls.Add(lblName)
                    itemPanel.Controls.Add(lblStock)
                    itemPanel.Controls.Add(lblPrice)
                    itemPanel.Controls.Add(btnSelect)

                    FlowLayoutPanel1.Controls.Add(itemPanel)

                End If
            Next
        Else
            Dim lbl As New Label With {
                .Text = "No products found.",
                .ForeColor = Color.Red,
                .AutoSize = True,
                .Font = New Font("Segoe UI", 12, FontStyle.Bold)
            }
            FlowLayoutPanel1.Controls.Add(lbl)
        End If
    End Sub
    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub
    Private Sub LoadCategories()
        Dim sql As String = "SELECT DISTINCT Category FROM Products"
        Dim dt As DataTable = ExecuteQuery(sql)

        Guna2ComboBoxCategory.Items.Clear()
        Guna2ComboBoxCategory.Items.Add("All")
        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                Guna2ComboBoxCategory.Items.Add(row("Category").ToString())
            Next
        End If
        Guna2ComboBoxCategory.SelectedIndex = 0
    End Sub
    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtSearch.TextChanged
        LoadProductsToFlow()
    End Sub
    Private Sub Guna2ComboBoxCategory_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2ComboBoxCategory.SelectedIndexChanged
        LoadProductsToFlow()
    End Sub
    Private Sub RemoveSelectedCartItems()
        Try
            If DgvCart.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select an item to remove.",
                                "Remove Item", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim confirmResult As DialogResult = MessageBox.Show(
                "Are you sure you want to remove this item from the cart?",
                "Confirm Remove",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If confirmResult = DialogResult.Yes Then
                For Each row As DataGridViewRow In DgvCart.SelectedRows
                    DgvCart.Rows.Remove(row)
                Next

                UpdateCartTotal()
            End If

        Catch ex As Exception
            MessageBox.Show("Error removing item: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub Guna2GradientButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton8.Click
        RemoveSelectedCartItems()
    End Sub
    Private Sub Guna2GradientButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ClearCart()
    End Sub
    Private Sub ClearCart()
        Try
            If DgvCart.Rows.Count = 0 Then
                MessageBox.Show("The cart is already empty.",
                                "Clear Cart", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If

            Dim confirmResult As DialogResult = MessageBox.Show(
                "Are you sure you want to clear all items from the cart?",
                "Confirm Clear",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If confirmResult = DialogResult.Yes Then
                DgvCart.Rows.Clear()
                UpdateCartTotal()
            End If

        Catch ex As Exception
            MessageBox.Show("Error clearing cart: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub UpdateCartTotal()
        Try
            Dim grandTotal As Decimal = 0

            ' Sum up all cart totals
            For Each row As DataGridViewRow In DgvCart.Rows
                If row.Cells("Total").Value IsNot Nothing Then
                    grandTotal += Convert.ToDecimal(row.Cells("Total").Value)
                End If
            Next

            ' Determine discount percent safely
            Dim discountPercent As Decimal = 0D
          

            ' Calculate discount
            Dim discountAmount As Decimal = grandTotal * discountPercent
            Dim totalAfterDiscount As Decimal = grandTotal - discountAmount

            ' Calculate VAT (12% of subtotal)
            Dim subtotal As Decimal = totalAfterDiscount / 1.12D
            Dim vat As Decimal = totalAfterDiscount - subtotal

            ' Update labels safely
            If LblDiscount IsNot Nothing Then LblDiscount.Text = "Discount: ₱" & discountAmount.ToString("N2")
            If LblSubtotal IsNot Nothing Then LblSubtotal.Text = "Subtotal: ₱" & subtotal.ToString("N2")
            If LblVAT IsNot Nothing Then LblVAT.Text = "VAT (12%): ₱" & vat.ToString("N2")
            If Lbltotal IsNot Nothing Then Lbltotal.Text = "Grand Total: ₱" & totalAfterDiscount.ToString("N2")

        Catch ex As Exception
            MessageBox.Show("Error calculating totals: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub Guna2GradientButton10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton10.Click
        ClearCart()
    End Sub
    ' Scanner
    Private Sub Guna2TextBox1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs)
 
    End Sub
    Private Sub HandleBarcodeScan(ByVal productName As String)
        Try
            If String.IsNullOrWhiteSpace(productName) Then Exit Sub

            Dim sqlCheck As String = "SELECT COUNT(*) FROM Products WHERE ProductName = @ProductName"
            AddParam("@ProductName", productName)
            Dim count As Integer = Convert.ToInt32(ExecuteScalar(sqlCheck))

            If count = 0 Then
                MessageBox.Show("Scanned Product Name does not exist in the system.",
                                "Invalid Product",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim sql As String = "SELECT ProductID, ProductName, Quantity, SellingPrice, PicturePhotoPath FROM Products WHERE ProductName = @ProductName AND Quantity > 0"
            AddParam("@ProductName", productName)
            Dim dt As DataTable = ExecuteQuery(sql)

            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                MessageBox.Show("Product not found or out of stock.",
                                "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim row As DataRow = dt.Rows(0)

            Dim qtyForm As New Quantity()
            qtyForm.ProductID = row("ProductID").ToString()
            qtyForm.ProdName = row("ProductName").ToString()
            qtyForm.ProdStock = Convert.ToInt32(row("Quantity"))
            qtyForm.ProdPrice = Convert.ToDecimal(row("SellingPrice"))
            qtyForm.ProdImage = If(IO.File.Exists(row("PicturePhotoPath").ToString()),
                                   Image.FromFile(row("PicturePhotoPath").ToString()),
                                   Nothing)

            If qtyForm.ShowDialog() = DialogResult.OK Then
                Dim selectedQty As Integer = qtyForm.SelectedQuantity
                Dim total As Decimal = qtyForm.ProdPrice * selectedQty
                Dim itemExists As Boolean = False

                For Each dgvRow As DataGridViewRow In DgvCart.Rows
                    If dgvRow.Cells("ID").Value IsNot Nothing AndAlso
                       dgvRow.Cells("ID").Value.ToString() = qtyForm.ProductID Then

                        Dim currentQty As Integer = Convert.ToInt32(dgvRow.Cells("Quantity").Value)
                        Dim newQty As Integer = Math.Min(currentQty + selectedQty, qtyForm.ProdStock)

                        dgvRow.Cells("Quantity").Value = newQty
                        dgvRow.Cells("Total").Value = (newQty * qtyForm.ProdPrice).ToString("N2")
                        itemExists = True
                        Exit For
                    End If
                Next

                If Not itemExists Then
                    DgvCart.Rows.Add(qtyForm.ProductID,
                                     qtyForm.ProdName,
                                     selectedQty,
                                     qtyForm.ProdPrice.ToString("N2"),
                                     total.ToString("N2"))
                End If

                UpdateCartTotal()
            End If

        Catch ex As Exception
            MessageBox.Show("Error handling barcode scan: " & ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Function GetCartTotal() As Decimal
        Dim grandTotal As Decimal = 0
        For Each row As DataGridViewRow In DgvCart.Rows
            If row.Cells("Total").Value IsNot Nothing Then
                grandTotal += Convert.ToDecimal(row.Cells("Total").Value)
            End If
        Next

        ' Apply discount
        Dim discountPercent As Decimal = 0D
    

        Dim discountAmount As Decimal = grandTotal * discountPercent
        Dim totalAfterDiscount As Decimal = grandTotal - discountAmount

        Return totalAfterDiscount
    End Function

    Private Sub GenerateReceipt(ByVal transactionID As Integer, ByVal cash As Decimal, ByVal change As Decimal, ByVal total As Decimal, ByVal discountAmount As Decimal)
        Try
            Dim receiptPath As String = "Receipt_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".txt"
            Using writer As New IO.StreamWriter(receiptPath)
                writer.WriteLine("      *** Ever Supermaarket ***")
                writer.WriteLine("           Closer to Home")
                writer.WriteLine("J.P. Rizal St., Maypajo, Caloocan City")
                writer.WriteLine(" ")
                writer.WriteLine("-------------------------------------")
                writer.WriteLine("Transaction ID: " & transactionID)
                writer.WriteLine("Date: " & DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"))
                writer.WriteLine("Cashier: " & CurrentUser)
                writer.WriteLine("-------------------------------------")
                writer.WriteLine("Item             Qty   Price   Total")
                writer.WriteLine("-------------------------------------")

                For Each row As DataGridViewRow In DgvCart.Rows
                    If row.Cells("ProductName").Value IsNot Nothing Then
                        writer.WriteLine(String.Format("{0,-15} {1,3} {2,7} {3,8}",
                            row.Cells("ProductName").Value.ToString(),
                            row.Cells("Quantity").Value.ToString(),
                            row.Cells("Price").Value.ToString(),
                            row.Cells("Total").Value.ToString()))
                    End If
                Next

                Dim noVAT As Decimal = total / 1.12D
                Dim vatAmount As Decimal = total - noVAT

                writer.WriteLine("-------------------------------------")
                writer.WriteLine("Discount:      ₱" & discountAmount.ToString("N2"))
                writer.WriteLine("Gross:         ₱" & noVAT.ToString("N2"))
                writer.WriteLine("VAT(12%):      ₱" & vatAmount.ToString("N2"))
                writer.WriteLine("Total:         ₱" & total.ToString("N2"))
                writer.WriteLine("-------------------------------------")
                writer.WriteLine("Cash:          ₱" & cash.ToString("N2"))
                writer.WriteLine("Change:        ₱" & change.ToString("N2"))
                writer.WriteLine("-------------------------------------")
                writer.WriteLine(" ")
                writer.WriteLine("    Thank you for your purchase!")
                writer.WriteLine("      Please come back again")
            End Using

            Process.Start("notepad.exe", receiptPath)

        Catch ex As Exception
            MessageBox.Show("Error generating receipt: " & ex.Message,
                            "Receipt Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Guna2GradientButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton9.Click
      Try
            Dim total As Decimal = GetCartTotal()

            Dim cash As Decimal
            If Not Decimal.TryParse(TxtAmount.Text, cash) Then
                MessageBox.Show("Please enter a valid amount.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            If cash < total Then
                MessageBox.Show("Insufficient payment. Cash must be greater than or equal to total.", "Payment Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            Dim change As Decimal = cash - total

            MessageBox.Show("Payment accepted!" & vbCrLf & "Change: ₱" & change.ToString("N2"), "Transaction Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' ✅ Update product stock
            For Each row As DataGridViewRow In DgvCart.Rows
                If row.IsNewRow Then Continue For
                Dim productName As String = row.Cells("ProductName").Value.ToString()
                Dim quantityBought As Integer = Convert.ToInt32(row.Cells("Quantity").Value)

                Dim sqlUpdate As String = "UPDATE Products SET Quantity = Quantity - @qty WHERE ProductName = @pname"
                AddParam("@qty", quantityBought)
                AddParam("@pname", productName)
                ExecuteNonQuery(sqlUpdate)
            Next

            ' ✅ Calculate gross and VAT
            Dim gross As Decimal = total / 1.12D
            Dim vat As Decimal = total - gross
            Dim cashier As String = If(String.IsNullOrEmpty(CurrentUser), "Unknown", CurrentUser)

            ' ✅ Insert transaction (1 line SQL)
            Dim sqlInsert As String = "INSERT INTO Transactions (DateAndTime, Cashier, Gross, Vat, Total) VALUES (@dt, @cashier, @gross, @vat, @total); SELECT SCOPE_IDENTITY();"

            AddParam("@dt", DateTime.Now)
            AddParam("@cashier", cashier)
            AddParam("@gross", gross)
            AddParam("@vat", vat)
            AddParam("@total", total)

            Dim transactionID As Object = ExecuteScalar(sqlInsert)
            If transactionID Is Nothing OrElse IsDBNull(transactionID) Then
                MessageBox.Show("Failed to insert transaction record.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            ' ✅ Clear cart and reset
            DgvCart.Rows.Clear()
            UpdateCartTotal()
            TxtAmount.Clear()



            LoadCategories()
            LoadProductsToFlow()
            MessageBox.Show("Transaction recorded successfully! Transaction ID: " & transactionID.ToString(), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub Guna2GradientButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton3.Click
        CashierInventory.Show()
        Me.Hide()
    End Sub

    Private Sub ComboBoxDiscount_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UpdateCartTotal()
    End Sub

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click
        History.Show()
        Me.Hide()
    End Sub

    Private Sub FlowLayoutPanel1_Paint(sender As System.Object, e As System.Windows.Forms.PaintEventArgs) Handles FlowLayoutPanel1.Paint

    End Sub

    Private Sub TxtAmount_TextChanged(sender As System.Object, e As System.EventArgs) Handles TxtAmount.TextChanged

    End Sub
End Class