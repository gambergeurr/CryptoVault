using Avalonia.Controls;
using Avalonia.Media;

namespace CryptoVault;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        tbxPasswordAnalyzer.Text = "";
    }

    private void AnalyzePassword(object? sender, TextChangedEventArgs e)
    {
        string password = tbxPasswordAnalyzer.Text;
        int score = 0;

        if (PasswordChecker.CheckCaps(password))
        {
            tbkCaps.Text = "☑ Majuscules";
            score++;
        }
        else
        {
            tbkCaps.Text  = "☒ Majuscules";
        }
        
        if (PasswordChecker.CheckLower(password))
        {
            tbkLower.Text = "☑ Minuscules";
            score++;
        }
        else
        {
            tbkLower.Text  = "☒ Minuscules";
        }
        
        if (PasswordChecker.CheckLenght(password))
        {
            tbkLenght.Text = "☑ Longueur (10 char.)";
            score++;
        }
        else
        {
            tbkLenght.Text  = "☒ Longueur";
        }
        
        if (PasswordChecker.CheckNumber(password))
        {
            tbkNumbers.Text = "☑ Chiffres";
            score++;
        }
        else
        {
            tbkNumbers.Text  = "☒ Chiffres";
        }
        
        if (PasswordChecker.CheckSpecial(password))
        {
            tbkSpecialChars.Text = "☑ Symboles";
            score++;
        }
        else
        {
            tbkSpecialChars.Text  = "☒ Symboles";
        }

        switch (score)
        {
            case <= 2:
                tbkStrenght.Text = "Faible";
                tbkStrenght.Foreground = Brushes.Red;
                break;
            case <= 4:
                tbkStrenght.Text = "Moyen";
                tbkStrenght.Foreground = Brushes.Orange;
                break;
            case 5:
                tbkStrenght.Text = "Fort";
                tbkStrenght.Foreground = Brushes.Green;
                break;
        }
    }
}