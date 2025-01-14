# RDO.WPF Overview

Built on top of RDO.Data, RDO.WPF is a presentation layer which can greatly help developers to build WPF desktop applications:

![image](/images/RdoWpfOverview.jpg)

## MVP Architecture

RDO.WPF is implemented as Model-View-Presenter (MVP) to improve the separation of concerns in presentation logic. The following is the architecture of RDO.WPF MVP:

[!include[Welcome to RDO.Net](../_rdo_wpf_mvp_architecture.md)]

## How to Use

Using RDO.WPF can be as simple as:

* Add nuget package [DevZest.Data.WPF](https://www.nuget.org/packages/DevZest.Data.WPF/) into your WPF project;
* Add a <xref:DevZest.Data.Views.DataView> control into your UI;
* Add your presenter class derived from <xref:DevZest.Data.Presenters.DataPresenter`1>, implement the abstract <xref:DevZest.Data.Presenters.DataPresenter`1.BuildTemplate*> method, which takes a <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder> parameter.
* Call <xref:DevZest.Data.Presenters.DataPresenter`1.Show*> method of your presenter class to show data to the view.

Here is an example of presenter class implementation:

# [C#](#tab/cs)

[!code-csharp[DetailPresenter](../../../../samples/AdventureWorksLT.WpfApp/SalesOrderWindow.DetailPresenter.cs)]

# [VB.Net](#tab/vb)

[!code-vb[DetailPresenter](../../../../samples.vb/AdventureWorksLT.WpfApp/SalesOrderWindow.DetailPresenter.vb)]

***

The above code produce the following data grid UI, with foreign key lookup and paste append from clipboard implemented:

![image](/images/SalesOrderDetailUI.jpg)
