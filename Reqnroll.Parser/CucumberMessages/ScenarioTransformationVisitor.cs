using System.Collections.Generic;
using System.Linq;
using Gherkin.Ast;

namespace Reqnroll.Parser.CucumberMessages;

abstract class ScenarioTransformationVisitor : GherkinDocumentVisitor
{
    protected ReqnrollDocument SourceDocument;
    private ReqnrollDocument _transformedDocument;
    private ReqnrollFeature _transformedFeature;
    private bool _hasTransformedScenarioInFeature = false;
    private bool _hasTransformedScenarioInCurrentRule = false;
    private readonly List<IHasLocation> _featureChildren = new();
    private readonly List<IHasLocation> _ruleChildren = new();
    private List<IHasLocation> _currentChildren;

    public ReqnrollDocument TransformDocument(ReqnrollDocument document)
    {
        Reset();
        AcceptDocument(document);
        return _transformedDocument ?? document;
    }

    private void Reset()
    {
        SourceDocument = null;
        _transformedDocument = null;
        _transformedFeature = null;
        _featureChildren.Clear();
        _ruleChildren.Clear();
        _hasTransformedScenarioInFeature = false;
        _hasTransformedScenarioInCurrentRule = false;
        _currentChildren = _featureChildren;
    }

    protected abstract Scenario GetTransformedScenarioOutline(ScenarioOutline scenarioOutline);
    protected abstract Scenario GetTransformedScenario(Scenario scenario);

    protected override void OnScenarioOutlineVisited(ScenarioOutline scenarioOutline)
    {
        var transformedScenarioOutline = GetTransformedScenarioOutline(scenarioOutline);
        OnScenarioVisitedInternal(scenarioOutline, transformedScenarioOutline);
    }

    protected override void OnScenarioVisited(Scenario scenario)
    {
        var transformedScenario = GetTransformedScenario(scenario);
        OnScenarioVisitedInternal(scenario, transformedScenario);
    }

    private void OnScenarioVisitedInternal(Scenario scenario, Scenario transformedScenario)
    {
        if (transformedScenario == null)
        {
            _currentChildren.Add(scenario);
            return;
        }

        _hasTransformedScenarioInFeature = true;
        _hasTransformedScenarioInCurrentRule = true;
        _currentChildren.Add(transformedScenario);
    }

    protected override void OnBackgroundVisited(Background background)
    {
        _currentChildren.Add(background);
    }

    protected override void OnRuleVisiting(Rule rule)
    {
        _ruleChildren.Clear();
        _hasTransformedScenarioInCurrentRule = false;
        _currentChildren = _ruleChildren;
    }

    protected override void OnRuleVisited(Rule rule)
    {
        _currentChildren = _featureChildren;
        if (_hasTransformedScenarioInCurrentRule)
        {
            var transformedRule = new Rule(
                rule.Tags?.ToArray() ?? [],
                rule.Location,
                rule.Keyword,
                rule.Name,
                rule.Description,
                _ruleChildren.ToArray());
            _featureChildren.Add(transformedRule);
        }
        else
        {
            _featureChildren.Add(rule);
        }
    }

    protected override void OnFeatureVisited(Feature feature)
    {
        if (_hasTransformedScenarioInFeature)
            _transformedFeature = new ReqnrollFeature(
                feature.Tags?.ToArray() ?? [],
                feature.Location,
                feature.Language,
                feature.Keyword,
                feature.Name,
                feature.Description,
                _featureChildren.ToArray());
    }

    protected override void OnDocumentVisiting(ReqnrollDocument document)
    {
        SourceDocument = document;
    }

    protected override void OnDocumentVisited(ReqnrollDocument document)
    {
        if (_transformedFeature != null)
            _transformedDocument = new ReqnrollDocument(_transformedFeature, document.Comments.ToArray(), document.DocumentLocation);
    }
}