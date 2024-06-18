namespace GameAutomation.Core;

// Значение в int, это погрешность при подсчете длины текста для каждого шрифта.
// Например, реальная длина текста = 290, но счетчик отдает 325.
// Значит, чтобы погасить погрешность, мы должны вычесть 35, как в случае Century Gothic.
// По тому же принципу и остальные шрифты.
public enum FontType
{
    Consolas = 0,
    TimesNewRoman = 1,
    LucidaSansUnicode = 2,
    Verdana = 10,
    CenturyGothic = 35,
}