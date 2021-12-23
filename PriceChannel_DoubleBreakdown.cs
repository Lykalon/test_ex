using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Indicators.Model;
using ScriptSolution.Model;

namespace ModulSolution.Robots
{
    internal class DoubleBreakdown : Script
    {
        public CreateInidicator ChannelUpLittle = new CreateInidicator(EnumIndicators.PriceChannel, 0, "Малый период");
        public CreateInidicator ChannelUpBig = new CreateInidicator(EnumIndicators.PriceChannel, 0, "Большой период");
        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
            "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");
        public ParamOptimization DistMax = new ParamOptimization(1, 5, 120, 5, "Расстояние между линиями MAX",
            "Максимальное расстояние между линиями индикатора.");
        public ParamOptimization DistMin = new ParamOptimization(1, 5, 120, 5, "Расстояние между линиями MIN",
            "Минимальное расстояние между линиями индикатора.");
        public ParamOptimization HLittleMax = new ParamOptimization(1, 5, 120, 5, "Максимальная ширина канала",
            "Максимальная ширина канала малого периода.");
        public ParamOptimization Reverse = new ParamOptimization(false, "Реверс",
            "Реверс - изменяет направление сделки (т.е. вместо открытия лонга будет шорт)");

        private string _dir { get; set; }
        public override void Execute()
        {
            if (Reverse.ValueBool)
            {
                for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (ChannelUpLittle.param.LinesIndicators[0].LineParam[0].Value < bar || ChannelUpBig.param.LineIndicators[0].LineParam[0].Value < bar)
                {
                    var priceUpLittle = ChannelUpLittle.param.LinesIndicators[0].PriceSeries;
                    var priceDownLittle = ChannelUpLittle.param.LinesIndicators[1].PriceSeries;
                    var priceUpBig = ChannelUpBig.param.LinesIndicators[0].PriceSeries;
                    var priceDownBig = ChannelUpBig.param.LinesIndicators[1].PriceSeries;

                    if (LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        if (LongPos.Count == 0 && _dir != "Short" && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) <= DistMax.Value
                                && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) >= DistMin.Value && priceUpLittle[bar + 1] - priceDownLittle[bar + 1] <= HLittleMax.Value
                                && (priceDownBig[bar + 1] - priceUpLittle[bar + 1] > 0 || priceDownLittle[bar + 1] - priceUpBig[bar + 1] > 0))
                            BuyGreater(bar + 1, priceUpLittle[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Открытие длинной позиции");
                        if (ShortPos.Count == 0 && _dir != "Long" && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) <= DistMax.Value
                                && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) >= DistMin.Value && priceUpLittle[bar + 1] - priceDownLittle[bar + 1] <= HLittleMax.Value
                                && (priceDownBig[bar + 1] - priceUpLittle[bar + 1] > 0 || priceDownLittle[bar + 1] - priceUpBig[bar + 1] > 0))
                            ShortLess(bar + 1, priceDownLittle[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Открытие короткой позиции");
                    }
                    // дополнить выход из позиций по логике
                    if (ShortPos.Count > 0)
                    {
                        CoverAtStop(bar + 1, ShortPos[0], priceUpLittle[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");                 
                    }
                    // дополнить выход из позиций по логике
                    if (LongPos.Count > 0)
                    {
                        SellAtStop(bar + 1, LongPos[0], priceDownLittle[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                    }

                    if (bar > 2)
                    {
                        ParamDebug("Верх. канал малый период тек.",
                            Math.Round(ChannelUpLittle.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                        ParamDebug("Ниж. канал малый период тек.",
                            Math.Round(ChannelUpLittle.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                        ParamDebug("Верх. канал большой период тек.",
                            Math.Round(ChannelUpBig.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                        ParamDebug("Ниж. канал большой период тек.",
                            Math.Round(ChannelUpBig.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                    }

                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {
                        if (LongPos.Count != 0)
                            _dir = "Long";
                        if (ShortPos.Count != 0)
                            _dir = "Short";

                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }
                    else
                    {
                        _dir = "";
                    }

                    ParamDebug("Направление", _dir);
                }
            }
            }
            else
            {
                for (var bar = IndexBar; bar < CandleCount - 1; bar++)
                {
                    if (ChannelUpLittle.param.LinesIndicators[0].LineParam[0].Value < bar || ChannelUpBig.param.LineIndicators[0].LineParam[0].Value < bar)
                    {
                        var priceUpLittle = ChannelUpLittle.param.LinesIndicators[0].PriceSeries;
                        var priceDownLittle = ChannelUpLittle.param.LinesIndicators[1].PriceSeries;
                        var priceUpBig = ChannelUpBig.param.LinesIndicators[0].PriceSeries;
                        var priceDownBig = ChannelUpBig.param.LinesIndicators[1].PriceSeries;

                        if (LongPos.Count == 0 && ShortPos.Count == 0)
                        {
                            if (ShortPos.Count == 0 && _dir != "Short" && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) <= DistMax.Value
                                    && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) >= DistMin.Value && priceUpLittle[bar + 1] - priceDownLittle[bar + 1] <= HLittleMax.Value
                                    && (priceDownBig[bar + 1] - priceUpLittle[bar + 1] > 0 || priceDownLittle[bar + 1] - priceUpBig[bar + 1] > 0))
                                BuyGreater(bar + 1, priceUpLittle[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                    "Открытие длинной позиции");
                            if (LongPos.Count == 0 && _dir != "Long" && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) <= DistMax.Value
                                    && Math.Abs(priceUpBig[bar + 1] - priceUpLittle[bar + 1]) >= DistMin.Value && priceUpLittle[bar + 1] - priceDownLittle[bar + 1] <= HLittleMax.Value
                                    && (priceDownBig[bar + 1] - priceUpLittle[bar + 1] > 0 || priceDownLittle[bar + 1] - priceUpBig[bar + 1] > 0))
                                ShortLess(bar + 1, priceDownLittle[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                    "Открытие короткой позиции");
                        }
                        // дополнить выход из позиций по логике
                        if (ShortPos.Count > 0)
                        {
                            CoverAtStop(bar + 1, ShortPos[0], priceUpLittle[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep,
                                "Переворот.");
                        }
                        // дополнить выход из позиций по логике
                        if (LongPos.Count > 0)
                        {
                            SellAtStop(bar + 1, LongPos[0], priceDownLittle[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                                "Переворот.");
                        }

                        if (bar > 2)
                        {
                            ParamDebug("Верх. канал малый период тек.",
                                Math.Round(ChannelUpLittle.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                            ParamDebug("Ниж. канал малый период тек.",
                                Math.Round(ChannelUpLittle.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                            ParamDebug("Верх. канал большой период тек.",
                                Math.Round(ChannelUpBig.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                            ParamDebug("Ниж. канал большой период тек.",
                                Math.Round(ChannelUpBig.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                        }

                        if (LongPos.Count != 0 || ShortPos.Count != 0)
                        {
                            if (LongPos.Count != 0)
                                _dir = "Long";
                            if (ShortPos.Count != 0)
                                _dir = "Short";

                            SendStandartStopFromForm(bar + 1, "");
                            SendTimePosCloseFromForm(bar + 1, "");
                            SendClosePosOnRiskFromForm(bar + 1, "");
                        }
                        else
                        {
                            _dir = "";
                        }

                        ParamDebug("Направление", _dir);
                    }
                }
            }
            
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1.0.0.0";
            DesParamStratetgy.DateRelease = "21.12.2021";
            DesParamStratetgy.DateChange = "21.12.2021";
            DesParamStratetgy.Author = "Сергей Абрамычев";

            DesParamStratetgy.Description = "Описание стратегии";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.NameStrategy = "Двойной пробой";

        }
    }
}