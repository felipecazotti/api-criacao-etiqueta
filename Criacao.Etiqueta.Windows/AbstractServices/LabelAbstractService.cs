using Criacao.Etiqueta.Windows.Models;

namespace Criacao.Etiqueta.Windows.AbstractServices;

public abstract class LabelAbstractService
{
    public const string A4251 = "A4251";
    public const string A4256 = "A4256";

    protected const decimal CONVERTER_CM_TO_POINT = 28.34645669291m;

    public abstract byte[] GenerateLabelsPdf(List<LabelInfo> labels);
}
