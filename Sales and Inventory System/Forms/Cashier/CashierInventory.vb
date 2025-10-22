Public Class CashierInventory

    Private Sub Guna2GradientButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton6.Click
        CashierDashboard.Show()
        Me.Hide()
    End Sub
    Private Sub Guna2GradientButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton4.Click
        Me.Hide()
        Sales.Show()
    End Sub
    Private Sub Guna2GradientButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        History.Show()
        Me.Hide()
    End Sub




    Private Sub CashierInventory_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Guna2HtmlLabel17.Text = DateTime.Now.ToString("MMMM dd, yyyy")
        Guna2HtmlLabel19.Text = CurrentUser
        LoadCategories()
        LoadProductsToFlow()
    End Sub



    Private Sub LoadProductsToFlow()
        FlowLayoutPanel1.Controls.Clear()

        Dim searchText As String = ""
        If TxtSearch IsNot Nothing Then searchText = TxtSearch.Text.Trim()

        Dim selectedCategory As String = "All"
        If ComboBoxCategory.SelectedItem IsNot Nothing Then
            selectedCategory = ComboBoxCategory.SelectedItem.ToString()
        End If

        Dim sql As String = "SELECT ProductName, Quantity, SellingPrice, PicturePhotoPath, Category FROM Products WHERE 1=1"

        If selectedCategory <> "All" Then
            sql &= " AND Category = @Category"
            AddParam("@Category", selectedCategory)
        End If

        If Not String.IsNullOrEmpty(searchText) Then
            sql &= " AND ProductName LIKE @Search"
            AddParam("@Search", "%" & searchText & "%")
        End If

        If Query(sql) AndAlso Data IsNot Nothing AndAlso Data.Tables.Count > 0 AndAlso Data.Tables(0).Rows.Count > 0 Then
            For Each row As DataRow In Data.Tables(0).Rows
                Dim photoPath As String = row("PicturePhotoPath").ToString()
                If IO.File.Exists(photoPath) Then
                    Dim itemPanel As New Guna.UI2.WinForms.Guna2GradientPanel() With {
                        .Width = 200,
                        .Height = 200,
                        .BorderColor = Color.FromArgb(159, 1, 1),
                        .BorderThickness = 2,
                        .BorderStyle = Drawing2D.DashStyle.Solid,
                        .BorderRadius = 15
                    }

                    Dim pb As New Guna.UI2.WinForms.Guna2PictureBox() With {
                        .Image = Image.FromFile(photoPath),
                        .Size = Guna2PictureBox1.Size,
                        .SizeMode = Guna2PictureBox1.SizeMode,
                        .Top = 20,
                        .Left = (itemPanel.Width - Guna2PictureBox1.Width) \ 2
                    }

                    Dim labelOffset As Integer = 23

                    Dim lblName As New Guna.UI2.WinForms.Guna2HtmlLabel() With {
                        .Text = row("ProductName").ToString(),
                        .Font = Guna2HtmlLabel2.Font,
                        .ForeColor = Guna2HtmlLabel2.ForeColor,
                        .Width = Guna2HtmlLabel2.Width,
                        .Top = pb.Bottom + 5,
                        .Left = 5 + labelOffset
                    }

                    Dim lblStock As New Guna.UI2.WinForms.Guna2HtmlLabel() With {
                        .Text = "Stock: " & row("Quantity").ToString(),
                        .Font = Guna2HtmlLabel3.Font,
                        .ForeColor = Guna2HtmlLabel3.ForeColor,
                        .Width = Guna2HtmlLabel3.Width,
                        .Top = lblName.Bottom,
                        .Left = 5 + labelOffset
                    }

                    Dim lblPrice As New Guna.UI2.WinForms.Guna2HtmlLabel() With {
                        .Text = "Price: ₱" & Convert.ToDecimal(row("SellingPrice")).ToString("F2"),
                        .Font = Guna2HtmlLabel9.Font,
                        .ForeColor = Guna2HtmlLabel9.ForeColor,
                        .Width = Guna2HtmlLabel9.Width,
                        .Top = lblStock.Bottom,
                        .Left = 5 + labelOffset
                    }

                    itemPanel.Controls.Add(pb)
                    itemPanel.Controls.Add(lblName)
                    itemPanel.Controls.Add(lblStock)
                    itemPanel.Controls.Add(lblPrice)

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

    Private Sub LoadCategories()
        Dim sql As String = "SELECT DISTINCT Category FROM Products"
        Dim dt As DataTable = ExecuteQuery(sql)

        ComboBoxCategory.Items.Clear()
        ComboBoxCategory.Items.Add("All")
        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                ComboBoxCategory.Items.Add(row("Category").ToString())
            Next
        End If
        ComboBoxCategory.SelectedIndex = 0
    End Sub

    Private Sub TxtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtSearch.TextChanged
        LoadProductsToFlow()
    End Sub

    Private Sub ComboBoxCategory_SelectedValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxCategory.SelectedValueChanged
        LoadProductsToFlow()
    End Sub

    Private Sub Guna2GradientButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton5.Click
        GlobalLogout()
    End Sub

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click
        History.Show()
        Me.Hide()
    End Sub

    Private Sub ComboBoxCategory_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ComboBoxCategory.SelectedIndexChanged

    End Sub


    Private Sub FlowLayoutPanel1_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles FlowLayoutPanel1.Paint

    End Sub

    Private Sub Guna2PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2PictureBox1.Click

    End Sub
End Class