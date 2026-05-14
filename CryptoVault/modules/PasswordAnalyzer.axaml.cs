using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace CryptoVault.modules;

/// <summary>
/// User control for analyzing password strength based on various heuristics.
/// </summary>
public partial class PasswordAnalyzer : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordAnalyzer"/> class.
    /// </summary>
    public PasswordAnalyzer()
    {
        InitializeComponent();
        tbxPasswordAnalyzer.Text = "";
    }
    
    /// <summary>
    /// Analyzes the password entered in the text box and updates the UI with the score and matching criteria.
    /// </summary>
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
            tbkLenght.Text  = "☒ Longueur (10 char.)";
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