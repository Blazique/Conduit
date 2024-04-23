
using Conduit.ApiClient;
using Conduit.Domain;
using Radix.Data;
using static Radix.Control.Option.Extensions;

namespace Conduit.Components;

[Route("/Profile/{username}")]
[InteractiveServerRenderMode(Prerender = true)]
public class Profile : Blazique.Web.Component
{
    [Inject]
    public GetProfile GetProfile { get; set; } = (_) => Task.FromResult(None<ProfileDto>());

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Parameter]
    public string Username { get; set; } = "";

    private Option<Domain.Profile> _profile = None<Domain.Profile>();

    protected override async Task OnInitializedAsync()
    {
        var profileOption = await GetProfile(Username);   
        _profile = profileOption.Map(profile => profile.ToProfile());
        await base.OnInitializedAsync();
    }

    public override Node[] Render()
    =>
    _profile switch
    {
        Some<Domain.Profile>(var profile) => 
            [        
                div([@class(["profile-page"])], [
                    div([@class(["user-info"])], [
                        div([@class(["container"])], [
                            div([@class(["row"])], [
                                div([@class(["col-xs-12", "col-md-10", "offset-md-1"])], [
                                    img([@class(["user-img"]), src([profile.Image])], []),
                                    h4([], [text(profile.Username)]),
                                    p([], [text(profile.Bio ?? "")]),
                                    button([@class(["btn", "btn-sm", "btn-outline-secondary", "action-btn"])], 
                                        [i([@class(["ion-plus-round"])], [text($" Follow {profile.Username}")])]),
                                    button([@class(["btn", "btn-sm", "btn-outline-secondary", "action-btn"])], 
                                        [i([@class(["ion-gear-a"]), on.click(_ => Navigation?.NavigateTo("/settings"))], [text(" Edit Profile Settings")]),

                                ])])
                            ])])])]),
                div([@class(["container"])], [
                    div([@class(["row"])], [
                        div([@class(["col-xs-12", "col-md-10", "offset-md-1"])], [
                            div([@class(["articles-toggle"])], [
                                ul([@class(["nav", "nav-pills", "outline-active"])], [
                                    li([@class(["nav-item"])], [
                                        a([@class(["nav-link", "active"]), href([""])], [text("My Articles")])
                                    ]),
                                    li([@class(["nav-item"])], [
                                        a([@class(["nav-link"]), href([""])], [text("Favorited Articles")])
                                    ])
                                ])
                            ]),
                            div([@class(["article-preview"])], [
                                div([@class(["article-meta"])], [
                                    a([href([$"/profile/{profile.Username}"])], [
                                        img([src(["http://i.imgur.com/Qr71crq.jpg"])], [])]),
                                    div([@class(["info"])], [
                                        a([@class(["author"]), href(["/profile/eric-simons"])], [
                                            text("Eric Simons")])],
                                        span([@class(["date"])], [text("January 20th")])),
                                    button([@class(["btn", "btn-outline-primary", "btn-sm", "pull-xs-right"])], [
                                        i([@class(["ion-heart"])], []),
                                ]),
                                a([href(["/article/how-to-buil-webapps-that-scale"]), @class(["preview-link"])], [
                                    h1([], [text("How to build webapps that scale")]),
                                    p([], [text("This is the description for the post.")]),
                                    span([], [text("Read more...")]),
                                    ul([@class(["tag-list"])], [
                                        li([@class(["tag-pill", "tag-default"])], [text("realworld")]),
                                        li([@class(["tag-pill", "tag-default"])], [text("implementations")])
                                    ])
                                ])
                            ])]),
                            div([@class(["article-preview"])], [
                                div([@class(["article-meta"])], [
                                    a([href(["/profile/albert-pai"])], [
                                        img([src(["http://i.imgur.com/N4VcUeJ.jpg"])], [])]),
                                    div([@class(["info"])], [
                                        a([@class(["author"]), href(["/profile/albert-pai"])], [
                                            text("Albert Pai")])],
                                        span([@class(["date"])], [text("January 20th")])),
                                    button([@class(["btn", "btn-outline-primary", "btn-sm", "pull-xs-right"])], [
                                        i([@class(["ion-heart"])], [])])]),
                                a([href(["/article/the-song-you"]), @class(["preview-link"])], [
                                    h1([], [text("The song you won't ever stop singing. No matter how hard you try.")]),
                                    p([], [text("This is the description for the post.")]),
                                    span([], [text("Read more...")]),
                                    ul([@class(["tag-list"])], [
                                        li([@class(["tag-pill", "tag-default"])], [text("Music")]),
                                        li([@class(["tag-pill", "tag-default"])], [text("Song")])
                                    ])
                                ])
                            ]),
                            ul([@class(["pagination"])], [
                                li([@class(["page-item"])], [
                                    a([@class(["page-link"]), href([""])], [text("1")])]),
                                li([@class(["page-item"])], [
                                    a([@class(["page-link"]), href([""])], [text("2")])])
                            ])
                        ])
                    ])
                ])
            ],
        _ => [
            div([@class(["alert", "alert-warning"])], [text("Profile not found")])
        ]
    };
}