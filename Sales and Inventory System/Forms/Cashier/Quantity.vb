Public Class Quantity
    Public Property ProductID As String
    Public Property ProdName As String
    Public Property ProdStock As Integer
    Public Property ProdPrice As Decimal
    Public Property ProdImage As Image
    Public Property SelectedQuantity As Integer = 1

    Private Sub Quantity_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Guna2HtmlLabel1.Text = "ID: " & ProductID
        Guna2HtmlLabelName.Text = ProdName
        Guna2HtmlLabelStock.Text = "Stock: " & ProdStock
        Guna2HtmlLabelPrice.Text = "Price: ₱" & ProdPrice.ToString("F2")
        Guna2PictureBoxProduct.Image = ProdImage

        numQuantity.Minimum = 1
        numQuantity.Maximum = Math.Max(1, ProdStock)
        numQuantity.Value = 1

    End Sub

    Private Sub Guna2GradientButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2GradientButton1.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Guna2Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Guna2Button1.Click
        SelectedQuantity = CInt(numQuantity.Value)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Guna2GradientPanel1_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Guna2GradientPanel1.Paint

    End Sub
End Class