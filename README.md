<div id="top"></div>
<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/bteixeira/apollobus">
    <img src="https://github.com/bteixeira691/ApolloBus/blob/master/Images/ApolloBus400.png" alt="Logo" width="400" height="300">
  </a>

<h3 align="center">ApolloBus</h3>

  <p align="center">
 ApolloBus is an event bus with integration with RabbitMq, Kafka, and Service Bus Azure.
    <br />
    <a href="https://github.com/bteixeira691/ApolloBus/wiki"><strong>Explore the wiki »</strong></a>
    <br />
    <br />
    <a href="https://github.com/bteixeira691/ApolloBus/tree/master/Samples">View Samples</a>
    ·
    <a href="https://github.com/bteixeira691/ApolloBus/issues">Report Bug</a>
    ·
    <a href="https://github.com/bteixeira691/ApolloBus/issues">Request Feature</a>
  </p>
</div>




<!-- ABOUT THE PROJECT -->
## About The Project

ApolloBus is an EventBus with integration with RabbitMq, Kafka and ServiceBus Azure. With a pub/sub pattern.
Use the appsettings for the configuration. You can see that in the samples.






### Built With

* [.Net](https://dotnet.microsoft.com/)
* [Kafka](https://kafka.apache.org/)
* [RabbitMq](https://www.rabbitmq.com/)
* [ServiceBus Azure](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)
* [AmazonSQS](https://aws.amazon.com/pt/sqs/)
* [Polly](https://github.com/App-vNext/Polly)
* [Serilog](https://serilog.net/)



<!-- GETTING STARTED -->
## Getting Started

To start you need just have to install the NuGetPackage

  ```sh
  Install-Package ApolloBus -Version x.x.x
  ```
You can check the versions here -> https://www.nuget.org/packages/ApolloBus/






<!-- ROADMAP -->
## Roadmap

- [] Retry Policy with Kafka
- [x] Retry Policy with ServiceBus Azure
- [x] Better Validation
    - [x] Kafka config
    - [x] ServiceBus Azure config
    - [x] RabbitMq config
- [] Deadqueue if dont exist a handler
- [] More services clients
- [] Multiple instance of clients

See the [open issues](https://github.com/bteixeira691/ApolloBus/issues) for a full list of proposed features (and known issues).





<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request





<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.



## Credits

Logo made by -> https://carsilva.weebly.com/

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/bteixeira691/ApolloBus.svg?style=for-the-badge
[contributors-url]: https://github.com/bteixeira691/ApolloBus/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/bteixeira691/ApolloBus.svg?style=for-the-badge
[forks-url]: https://github.com/bteixeira691/ApolloBus/network/members
[stars-shield]: https://img.shields.io/github/stars/bteixeira691/ApolloBus.svg?style=for-the-badge
[stars-url]: https://github.com/bteixeira691/ApolloBus/stargazers
[issues-shield]: https://img.shields.io/github/issues/bteixeira691/ApolloBus.svg?style=for-the-badge
[issues-url]: https://github.com/bteixeira691/ApolloBus/issues
[license-shield]: https://img.shields.io/github/license/bteixeira691/apollobus?label=license&style=for-the-badge
[license-url]: https://github.com/bteixeira691/ApolloBus/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/bernardojmteixeira
[product-screenshot]: images/screenshot.png
